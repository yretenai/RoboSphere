using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Core;
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
    public class RoboService : IDisposable
    {
        public RoboService(IDiscordRestChannelAPI channelApi, SqlContext sql, ILogger<RoboService> logger)
        {
            ChannelApi = channelApi;
            Sql = sql;
            Logger = logger;

            sql.Database.EnsureCreated();

            var crisis = sql.Settings.FirstOrDefault(x => x.Key == "CrisisChannels")?.Value;
            if (!string.IsNullOrEmpty(crisis))
                CrisisChannels = new HashSet<Snowflake>(crisis.Split(",").Select(x => new Snowflake(ulong.Parse(x))));
            else if (crisis == null)
                sql.Settings.Add(new Setting
                {
                    Key = "CrisisChannels",
                    Value = ""
                });

            sql.SaveChanges();
        }

        private IDiscordRestChannelAPI ChannelApi { get; }
        private SqlContext Sql { get; }
        private ILogger<RoboService> Logger { get; }

        private HashSet<Snowflake> CrisisChannels { get; } = new();

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            DisposeInner();
        }

        ~RoboService() => DisposeInner();

        public bool AddCrisisChannel(Snowflake id) => CrisisChannels.Add(id);

        public bool RemoveCrisisChannel(Snowflake id) => !CrisisChannels.Remove(id);

        public async Task Crisis(string message, CancellationToken? ct = null)
        {
            var limit = 2000;
            if (message.Length > limit)
            {
                if (message.StartsWith("```")) limit -= 3;
                message = message.Substring(0, limit - 3) + "...";
                if (message.StartsWith("```")) message += "```";
            }

            Logger.LogInformation($"Sending crisis message \"{message}\" to channels {string.Join(", ", CrisisChannels)}");
            foreach (var channelId in CrisisChannels)
            {
                IResult result = await ChannelApi.CreateMessageAsync(channelId, message, ct: ct ?? CancellationToken.None);
                if (result.IsSuccess) continue;

                Logger.LogError($"Cannot send message: {result.Error?.Message}");
                while (result.Inner != null)
                {
                    Logger.LogError($"Cannot send message: {result.Inner.Error?.Message}");
                    result = result.Inner;
                }
            }
        }

        public Task Crisis(Exception e, CancellationToken? ct = null) => Crisis($"```{e}```", ct);

        public async Task Commit(CancellationToken ct)
        {
            var crisis = await Sql.Settings.FirstAsync(x => x.Key == "CrisisChannels", ct);
            crisis.Value = string.Join(',', CrisisChannels);
            Sql.Settings.Update(crisis);
            await Sql.SaveChangesAsync(ct);
        }

        private void DisposeInner()
        {
            Commit(CancellationToken.None).Wait();
            Sql.Dispose();
        }
    }
}
