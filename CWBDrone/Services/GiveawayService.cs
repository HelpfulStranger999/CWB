using CWBDrone.Config;
using Discord;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CWBDrone.Services
{
    public class GiveawayService : DefaultService
    {
        protected Configuration Config { get; }
        protected List<ConfigGiveaway> Giveaways { get; } = new List<ConfigGiveaway>();

        public GiveawayService(Configuration config) => Config = config;

        public async Task Create(TimeSpan time, IUserMessage message, long winners, string prize)
        {
            var text = message.Channel as ITextChannel ?? throw new ArgumentNullException(nameof(message.Channel));

            var cg = new ConfigGiveaway
            {
                MessageID = message.Id,
                WinnerCount = winners,
                Prize = prize,
                GiveawayDuration = (long)time.TotalMilliseconds,
            };

            cg.SetTimer(text, time);
            Config.Guilds[text.GuildId].Giveaways.Add(cg);
            await Config.Write(DatabaseType.Guild);
            Giveaways.Add(cg);
        }

        public async Task<IUser[]> Reroll(IUserMessage message)
        {
            return await Config.Guilds[(message.Channel as IGuildChannel)?.GuildId 
                ?? throw new ArgumentNullException(nameof(message.Channel))]
                .Giveaways.Find(cg => cg.MessageID == message.Id)
                .Run(message.Channel as ITextChannel);
        }

        public async Task<ConfigGiveaway> Cancel(IUserMessage message)
        {
            var guild = Config.Guilds[(message.Channel as ITextChannel)?.GuildId
                ?? throw new ArgumentNullException(nameof(message.Channel))];
            var giveaway = guild.Giveaways.Find(cg => cg.MessageID == message.Id);
            giveaway.RunTimer.Change(Timeout.Infinite, Timeout.Infinite);
            guild.Giveaways.Remove(giveaway);
            await Config.Write(DatabaseType.Guild);
            return giveaway;
        }

        public override Task Disconnect()
        {
            foreach (var cg in Giveaways)
            {
                cg.RunTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
            return Task.CompletedTask;
        }

    }
}
