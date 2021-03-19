using JetBrains.Annotations;
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
using System.Threading;
using System.Threading.Tasks;

namespace RoboSphere.Discord.Commands
{
    [Group("admin")]
    [PublicAPI]
    public class RoboCommands : CommandGroup
    {
        public RoboCommands(IDiscordRestChannelAPI channelApi, RoboService robo, TrackingService data, ICommandContext context)
        {
            ChannelApi = channelApi;
            Robo = robo;
            Data = data;
            Context = context;
        }

        private IDiscordRestChannelAPI ChannelApi { get; }
        private RoboService Robo { get; }
        private TrackingService Data { get; }
        private ICommandContext Context { get; }

        [Command("add-crisis")]
        [SecureRoleCondition(346445994927587328, 683934826461528103)]
        public async Task<IResult> AddCrisis()
        {
            var msgReference = new Optional<IMessageReference>();
            if(Context is MessageContext msgContext)
            {
                msgReference = new MessageReference(msgContext.MessageID, Context.ChannelID, msgContext.Message.GuildID, false);
            }
            return await ChannelApi.CreateMessageAsync(Context.ChannelID, Robo.AddCrisisChannel(Context.ChannelID) ? "Added this channel as an emergency channel." : "Channel is already a crisis channel.", messageReference: msgReference);
        }

        [Command("remove-crisis")]
        [SecureRoleCondition(346445994927587328, 683934826461528103)]
        public async Task<IResult> RemoveCrisis()
        {
            var msgReference = new Optional<IMessageReference>();
            if(Context is MessageContext msgContext)
            {
                msgReference = new MessageReference(msgContext.MessageID, Context.ChannelID, msgContext.Message.GuildID, false);
            }
            return await ChannelApi.CreateMessageAsync(Context.ChannelID, Robo.RemoveCrisisChannel(Context.ChannelID) ? "Removed this channel as an emergency channel." : "Channel is not a crisis channel.", messageReference: msgReference);
        }
        
        [Command("say")]
        [SecureRoleCondition(346445994927587328, 683934826461528103)]
        public async Task<IResult> Say(string? channelId = null, string? message = null)
        {
            var msgReference = new Optional<IMessageReference>();
            if(Context is MessageContext msgContext)
            {
                msgReference = new MessageReference(msgContext.MessageID, Context.ChannelID, msgContext.Message.GuildID, false);
            }
            if (string.IsNullOrWhiteSpace(channelId))
            {
                return await ChannelApi.CreateMessageAsync(Context.ChannelID, EmoteCache.MidLifeJensen, messageReference: msgReference);
            }

            var id = Context.ChannelID.Value;
            if (string.IsNullOrWhiteSpace(message))
            {
                message = channelId;
            }
            else
            {
                id = ulong.Parse(channelId);
            }

            if (id == Context.ChannelID.Value && msgReference.HasValue && msgReference.Value.MessageID.HasValue)
            {
                await ChannelApi.DeleteMessageAsync(Context.ChannelID, msgReference.Value.MessageID.Value);
            }
            return await ChannelApi.CreateMessageAsync(new Snowflake(id), message);
        }

        [Command("commit")]
        [SecureRoleCondition(346445994927587328, 683934826461528103)]
        public async Task<IResult> Commit()
        {
            try
            {
                await Robo.Commit(CancellationToken.None);
                await Data.Commit(CancellationToken.None);
            }
            catch (Exception e)
            {
                await Robo.Crisis(e);
            }

            return Result.FromSuccess();
        }
    }
}
