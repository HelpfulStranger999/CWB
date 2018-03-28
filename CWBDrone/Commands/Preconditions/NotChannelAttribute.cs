using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CWBDrone.Commands.Preconditions
{
    public class NotChannelAttribute : RequireContextAttribute
    {
        public ulong[] ChannelIDs { get; set; }
        public string[] ChannelNames { get; set; }


        public NotChannelAttribute(params ulong[] channelIDs) : this(channelIDs, new string[0]) { }
        public NotChannelAttribute(params string[] channelNames) : this(new ulong[0], channelNames) { }

        public NotChannelAttribute(ulong[] channelIDs, string[] channelNames) : base(ContextType.Guild)
        {
            ChannelIDs = channelIDs;
            ChannelNames = channelNames;
        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var guildChannels = await context.Guild.GetTextChannelsAsync();
            var channels = from c in guildChannels
                           where ChannelIDs.Contains(c.Id) || ChannelNames.Contains(c.Name)
                           select c;

            return guildChannels.Intersect(channels).Any() ?
                PreconditionResult.FromError($"Forbidden channel!") :
                PreconditionResult.FromSuccess();
        }
    }
}
