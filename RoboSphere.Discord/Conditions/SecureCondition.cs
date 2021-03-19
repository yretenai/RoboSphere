using JetBrains.Annotations;
using Remora.Commands.Conditions;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Core;
using Remora.Results;
using RoboSphere.Discord.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RoboSphere.Discord.Conditions
{
    [PublicAPI]
    public class SecureCondition : ICondition<SecureRoleConditionAttribute>
    {
        public SecureCondition(IDiscordRestGuildAPI guildApi, IDiscordRestChannelAPI channelApi, ICommandContext context)
        {
            GuildApi = guildApi;
            ChannelApi = channelApi;
            Context = context;
        }

        private IDiscordRestGuildAPI GuildApi { get; }
        private IDiscordRestChannelAPI ChannelApi { get; }
        private ICommandContext Context { get; }

        public async ValueTask<Result> CheckAsync(SecureRoleConditionAttribute conditionAttribute, CancellationToken ct = new())
        {
            var getChannel = await ChannelApi.GetChannelAsync(Context.ChannelID, ct);
            if (!getChannel.IsSuccess) return Result.FromError(getChannel);
            var channel = getChannel.Entity;

            if (channel.Type != ChannelType.GuildText || !channel.GuildID.HasValue)
            {
                await ChannelApi.CreateMessageAsync(Context.ChannelID, "Use me in a server " + EmoteCache.PicardFacepalm, ct: ct);
                return new GenericError("Used in a DM");
            }

            var getMember = await GuildApi.GetGuildMemberAsync(channel.GuildID.Value, Context.User.ID, ct);
            if (!getMember.IsSuccess) return Result.FromError(getChannel);
            var member = getMember.Entity;

            if (member.Roles.Any(x => conditionAttribute.RoleIds.Contains(x))) return Result.FromSuccess();

            var msgReference = new Optional<IMessageReference>();
            if(Context is MessageContext msgContext)
            {
                msgReference = msgContext.Message.MessageReference;
            }
            await ChannelApi.CreateMessageAsync(Context.ChannelID, "You wish you could use this wouldn't you " + EmoteCache.MeguFace, messageReference: msgReference, ct: ct);
            return new GenericError("No Role");
        }
    }
}
