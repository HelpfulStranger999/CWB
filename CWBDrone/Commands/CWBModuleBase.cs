using CWBDrone.Config;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using System;

namespace CWBDrone.Commands
{
    public abstract class CWBModuleBase : ModuleBase<CWBContext>
    {
        public DiscordRestClient Rest { get; set; }
        public DiscordSocketClient Client { get; set; }
        public CommandService Commands { get; set; }
        public IServiceProvider Services { get; set; }
        public CWBDrone Bot { get; set; }
        public Configuration Configuration { get; set; }

        protected new virtual void BeforeExecute(CommandInfo info)
        {
            base.BeforeExecute(info);
            Context.Channel.TriggerTypingAsync().GetAwaiter().GetResult();
        }
    }
}
