using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CWBDrone.Config;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;

namespace CWBDrone.Commands
{
    public abstract class DefaultListener : IListener
    {
        public CWBContext Context { get; set; }
        public DiscordRestClient Rest { get; set; }
        public DiscordSocketClient Client { get; set; }
        public CommandService Commands { get; set; }
        public IServiceProvider Services { get; set; }
        public CWBDrone Bot { get; set; }
        public Configuration Configuration { get; set; }

        public abstract Task ExecuteAsync();

        public RunMode GetRunMode()
        {
            return RunMode.Sync;
        }
    }
}
