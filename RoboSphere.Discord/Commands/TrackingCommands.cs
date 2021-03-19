using DragonLib;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Core;
using Remora.Results;
using RoboSphere.Discord.Conditions;
using RoboSphere.Discord.Data;
using RoboSphere.Discord.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RoboSphere.Discord.Commands
{
    [Group("tracking")]
    [PublicAPI]
    public class TrackingCommands : CommandGroup
    {
        public TrackingCommands(IDiscordRestChannelAPI channelApi, ILogger<TrackingCommands> logger, ICommandContext context, RoboService robo, TrackingService data, Settings settings)
        {
            ChannelApi = channelApi;
            Logger = logger;
            Context = context;
            Robo = robo;
            Data = data;
            Settings = settings;
        }

        private IDiscordRestChannelAPI ChannelApi { get; }
        private ILogger<TrackingCommands> Logger { get; }
        private ICommandContext Context { get; }
        private RoboService Robo { get; }
        private TrackingService Data { get; }
        public Settings Settings { get; }

        [Command("when")]
        [SecureRoleCondition(346445994927587328, 683934826461528103)]
        public async Task<IResult> When(string? mode = null)
        {
            var msgReference = new Optional<IMessageReference>();
            if (Context is MessageContext msgContext)
            {
                msgReference = new MessageReference(msgContext.MessageID, Context.ChannelID, msgContext.Message.GuildID, false);
            }

            if (string.IsNullOrEmpty(mode))
            {
                return await ChannelApi.CreateMessageAsync(Context.ChannelID, EmoteCache.MidLifeJensen, messageReference: msgReference);
            }

            var stopwatch = await Data.GetStopwatch(mode.ToUpper().Split(' ').FirstOrDefault() ?? "");
            if (stopwatch.Next == 0)
            {
                return await ChannelApi.CreateMessageAsync(Context.ChannelID, $"Tracking {mode} has not been processed yet", messageReference: msgReference);
            }
            return await ChannelApi.CreateMessageAsync(Context.ChannelID, $"{mode} is due {DateTimeOffset.UtcNow.RelativeTime(stopwatch.Next)} ({DateTimeOffset.FromUnixTimeMilliseconds(stopwatch.Next):g})", messageReference: msgReference);
        }
        
        [Command("toggle")]
        [SecureRoleCondition(346445994927587328, 683934826461528103)]
        public async Task<IResult> ToggleData(string? id = null)
        {
            var msgReference = new Optional<IMessageReference>();
            if(Context is MessageContext msgContext)
            {
                msgReference = new MessageReference(msgContext.MessageID, Context.ChannelID, msgContext.Message.GuildID, false);
            }

            if (string.IsNullOrEmpty(id))
            {
                return await ChannelApi.CreateMessageAsync(Context.ChannelID, EmoteCache.MidLifeJensen, messageReference: msgReference);
            }
            
            id = id.ToUpper();
            if (Data.Tracking.Contains(id))
            {
                Data.Tracking.Remove(id);
                return await ChannelApi.CreateMessageAsync(Context.ChannelID, $"Stopped tracking {id}", messageReference: msgReference);
            }

            Data.Tracking.Add(id);
            return await ChannelApi.CreateMessageAsync(Context.ChannelID, $"Tracking {id}", messageReference: msgReference);
        }


        [Command("do")]
        [SecureRoleCondition(346445994927587328, 683934826461528103)]
        public async Task<IResult> Persistence(string? mode = null)
        {
            var msgReference = new Optional<IMessageReference>();
            if(Context is MessageContext msgContext)
            {
                msgReference = new MessageReference(msgContext.MessageID, Context.ChannelID, msgContext.Message.GuildID, false);
            }
            switch (mode)
            {
                default:
                {
                    return await ChannelApi.CreateMessageAsync(Context.ChannelID, EmoteCache.MidLifeJensen, messageReference: msgReference);
                }
            }
        }
    }
}
