using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remora.Commands.Extensions;
using Remora.Discord.Caching.Extensions;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Gateway;
using Remora.Discord.Gateway.Extensions;
using Remora.Discord.Gateway.Results;
using Remora.Results;
using RoboSphere.Discord.Commands;
using RoboSphere.Discord.Conditions;
using RoboSphere.Discord.Data;
using RoboSphere.Discord.Services;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RoboSphere.Discord
{
    [PublicAPI]
    public class Program
    {
        private static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            var cts = new CancellationTokenSource();

            Console.CancelKeyPress += (_, @event) =>
            {
                @event.Cancel = true;
                cts.Cancel();
            };

            var token = Environment.GetEnvironmentVariable("SPHERICAL_TOKEN") ?? throw new Exception("provide env SPHERICAL_TOKEN");
            Settings settings = Settings.Default;
            if (File.Exists("Settings.json")) settings = JsonSerializer.Deserialize<Settings>(await File.ReadAllTextAsync("Settings.json", cts.Token)) ?? settings;

            var serviceCollection = new ServiceCollection()
                .AddLogging(c => c.AddConsole()
                    .AddFilter("System.Net.Http.HttpClient.*.LogicalHandler", LogLevel.Warning)
                    .AddFilter("System.Net.Http.HttpClient.*.ClientHandler", LogLevel.Warning))
                .AddDiscordGateway(_ => token).AddDiscordCommands()
                .AddCommandResponder(options => options.Prefix = $"<@!{settings.ClientId}> ")
                .AddCondition<SecureCondition>()
                .AddCommandGroup<TrackingCommands>()
                .AddSingleton(settings)
                .AddSingleton<SqlContext>()
                .AddSingleton<RoboService>()
                .AddSingleton<TrackingService>()
                .AddCommandGroup<RoboCommands>()
                .AddCommandGroup<GeneralCommands>()
                .AddDiscordCaching();

            serviceCollection.AddHttpClient();

            var services = serviceCollection.BuildServiceProvider(true);

            var log = services.GetRequiredService<ILogger<Program>>();

            var gatewayClient = services.GetRequiredService<DiscordGatewayClient>();
            var persistentData = services.GetRequiredService<TrackingService>();

            var runResults = await Task.WhenAll(gatewayClient.RunAsync(cts.Token), persistentData.RunAsync(cts.Token));
            foreach (var result in runResults)
            {
                if (!result.IsSuccess)
                    switch (result.Error)
                    {
                        case ExceptionError exe:
                        {
                            log.LogError(exe.Exception, "Exception during gateway connection: {ExceptionMessage}", exe.Message);

                            break;
                        }
                        case GatewayWebSocketError:
                        case GatewayDiscordError:
                        {
                            log.LogError("Gateway error: {Message}", result.Error.Message);
                            break;
                        }
                        default:
                        {
                            log.LogError("Unknown error: {Message}", result.Error.Message);
                            break;
                        }
                    }
            }

            var sql = services.GetRequiredService<SqlContext>();
            await sql.SaveChangesAsync(cts.Token);
            await sql.DisposeAsync();

            log.LogInformation("Bye bye");
        }
    }
}
