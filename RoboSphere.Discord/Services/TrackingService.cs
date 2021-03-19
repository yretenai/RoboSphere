using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Remora.Results;
using RoboSphere.Discord.Data;
using RoboSphere.Discord.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RoboSphere.Discord.Services
{
    public class TrackingService : IDisposable
    {
        public TrackingService(SqlContext sql, RoboService robo, ILogger<TrackingService> logger, Settings settings)
        {
            Sql = sql;
            Robo = robo;
            Logger = logger;
            Settings = settings;

            Sql.Database.EnsureCreated();

            var tracking = Sql.Settings.FirstOrDefault(x => x.Key == "Tracking")?.Value;
            if (!string.IsNullOrEmpty(tracking))
                Tracking = new HashSet<string>(tracking.ToUpper().Split(","));
            else if (tracking == null)
                Sql.Settings.Add(new Setting
                {
                    Key = "Tracking",
                    Value = ""
                });

            Sql.SaveChanges();
        }

        private SqlContext Sql { get; }
        private RoboService Robo { get; }
        private ILogger<TrackingService> Logger { get; }
        private Settings Settings { get; }
        public HashSet<string> Tracking { get; } = new();

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            DisposeInner();
        }

        ~TrackingService() => DisposeInner();

        private record Timekeeper(Func<Settings, SqlContext, Task<bool>> Test, Func<Settings, SqlContext, Task> Work, TimeSpan Delay);

#pragma warning disable 1998
        private static readonly IReadOnlyDictionary<string, Timekeeper> Tracked = new Dictionary<string, Timekeeper>
        {
        };
#pragma warning restore 1998
        
        public async Task<Result> RunAsync(CancellationToken ct)
        {
            Logger.LogInformation("Starting data loop...");

            try
            {
                var firstRun = true;
                while (!ct.IsCancellationRequested)
                {
                    foreach (var (key, (test, work, delay)) in Tracked)
                    {
                        if (!Tracking.Contains(key) || !await test(Settings, Sql)) continue;
                        
                        var stopwatch = await GetStopwatch(key, ct);

                        if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() <= stopwatch.Next)
                        {
                            if (firstRun)
                            {
                                Logger.LogInformation($"{key} at {DateTimeOffset.FromUnixTimeMilliseconds(stopwatch.Next):G}");
                            }
                            continue;
                        }
                        
                        stopwatch.Next = DateTimeOffset.UtcNow.Add(delay).ToUnixTimeMilliseconds();
                        Logger.LogInformation($"{key} cycle, next at {DateTimeOffset.FromUnixTimeMilliseconds(stopwatch.Next):G}");
                        Sql.Stopwatch.Update(stopwatch);
                        try
                        {
                            await work(Settings, Sql);
                        }
                        catch (Exception e)
                        {
                            await Robo.Crisis(e);
                        }
                    }

                    await Robo.Commit(CancellationToken.None);
                    await Commit(CancellationToken.None);

                    await Task.Delay(TimeSpan.FromMinutes(5), ct);

                    firstRun = false;
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception e)
            {
                await Robo.Crisis(e, ct);
                return Result.FromError(e);
            }

            await Robo.Commit(CancellationToken.None);
            await Commit(CancellationToken.None);

            return Result.FromSuccess();
        }

        public async Task<Stopwatch> GetStopwatch(string key, CancellationToken? ct = null)
        {
            return await Sql.Stopwatch.FirstOrDefaultAsync(x => x.Key == key, ct ?? CancellationToken.None) ?? new Stopwatch
            {
                Key = key,
                Next = 0
            };
        }

        public async Task Commit(CancellationToken ct)
        {
            var tracking = await Sql.Settings.FirstAsync(x => x.Key == "Tracking", ct);
            tracking.Value = string.Join(",", Tracking).ToUpper();
            Sql.Settings.Update(tracking);
            await Sql.SaveChangesAsync(ct);
        }

        private void DisposeInner()
        {
            Commit(CancellationToken.None).Wait();
            Sql.Dispose();
        }
    }
}
