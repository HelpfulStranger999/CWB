using CWBDrone.Config;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace CWBDrone.Commands
{
    public class CWBContext : ICommandContext
    {
        public DiscordSocketClient Client { get; }
        public IGuild Guild { get; }
        public ITextChannel TextChannel { get; }
        public IMessageChannel Channel { get; }
        public IUser User { get; }
        public IGuildUser GuildUser { get; }
        public CWBDrone Bot { get; }
        public IUserMessage Message { get; }
        public string Prefix { get; }
        public ConfigGuild ConfigGuild { get; }

        public CWBContext(CWBDrone bot, IUserMessage message)
        {
            Client = bot.Socket;
            Channel = message.Channel;
            TextChannel = message.Channel as ITextChannel;
            Guild = TextChannel?.Guild;
            User = message.Author;
            GuildUser = User as IGuildUser;
            ConfigGuild = Guild != null ? bot.Configuration.Guilds.GetGuild(Guild.Id) : null;
            Bot = bot;
            Message = message;
            Prefix = ConfigGuild?.Prefix ?? CWBDrone.Prefix;
        }

        IDiscordClient ICommandContext.Client => Client;
    }
}
