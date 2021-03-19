using JetBrains.Annotations;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Core;
using Remora.Results;
using RoboSphere.Discord.Data;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace RoboSphere.Discord.Commands
{
    [PublicAPI]
    public class GeneralCommands : CommandGroup
    {
        public GeneralCommands(IDiscordRestChannelAPI channelApi, ICommandContext context)
        {
            ChannelApi = channelApi;
            Context = context;
        }

        private IDiscordRestChannelAPI ChannelApi { get; }
        private ICommandContext Context { get; }

        [Command("about")]
        public async Task<IResult> About()
        {
            var msgReference = new Optional<IMessageReference>();
            if (Context is MessageContext msgContext)
            {
                msgReference = new MessageReference(msgContext.MessageID, Context.ChannelID, msgContext.Message.GuildID, false);
            }

#if DEBUG
            var release = "Debug";
#else
            var release = "Release";
#endif
            return await ChannelApi.CreateMessageAsync(Context.ChannelID, $"Robo-Sphere v{Assembly.GetExecutingAssembly().GetName().Version} ({GitHelper.GetHash(Environment.CurrentDirectory)} @ {Environment.MachineName})\n{Environment.OSVersion.VersionString} .NET {Environment.Version} {release}", messageReference: msgReference);
        }

        [Command("ping")]
        public async Task<IResult> Ping()
        {
            var msgReference = new Optional<IMessageReference>();
            if(Context is MessageContext msgContext)
            {
                msgReference = new MessageReference(msgContext.MessageID, Context.ChannelID, msgContext.Message.GuildID, false);
            }
            return await ChannelApi.CreateMessageAsync(Context.ChannelID, EmoteCache.MeguFace, messageReference: msgReference);
        }
        
        [Command("rtx")]
        public async Task<IResult> RTX()
        {
            var msgReference = new Optional<IMessageReference>();
            if(Context is MessageContext msgContext)
            {
                msgReference = new MessageReference(msgContext.MessageID, Context.ChannelID, msgContext.Message.GuildID, false);
            }

            var random = new Random();
            return await ChannelApi.CreateMessageAsync(Context.ChannelID, random.NextDouble() > 0.5 ? EmoteCache.Jensen : EmoteCache.MidlifeJensen, messageReference: msgReference);
        }
    }
}
