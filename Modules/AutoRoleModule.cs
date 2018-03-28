using CWBDrone.Config;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWBDrone.Modules
{
    public class AutoRoleModule
    {
        public DiscordRestClient Rest { get; set; }
        public DiscordSocketClient Client { get; set; }
        public CommandService Commands { get; set; }
        public IServiceProvider Services { get; set; }
        public CWBDrone Bot { get; set; }
        public Configuration Configuration { get; set; }

        public async Task AutoRoleAsync(SocketGuildUser user)
        {
            var configGuild = Configuration.Guilds[user.Guild.Id];
            if (user.IsBot)
            {
                var roles = user.Guild.Roles.Where(r => configGuild.BotAutoRoles.Contains(r.Id));
                await user.AddRolesAsync(roles);
            }
            else
            {
                var roles = user.Guild.Roles.Where(r => configGuild.AutoRoles.Contains(r.Id));
                await user.AddRolesAsync(roles);
            }
        }
    }
}
