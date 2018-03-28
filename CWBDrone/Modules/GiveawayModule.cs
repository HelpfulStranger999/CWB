using CWBDrone.Commands;
using CWBDrone.Commands.Preconditions;
using CWBDrone.Services;
using CWBDrone.Tools;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CWBDrone.Modules
{
    [Group("giveaway")]
    public class GiveawayModule : CWBModuleBase
    {
        public GiveawayService Giveaways { get; set; }

        [Command]
        public async Task List()
        {
            var embed = new EmbedBuilder();
            foreach (var giveaway in Context.ConfigGuild.Giveaways)
            {
                embed.Description += 
            }
        }

        [Command("create"), Alias("start")]
        public async Task Create([Name("Duration")]TimeSpan time,
                                 [Name("Prize")][Remainder]string prize)
            => await Create(time, Context.TextChannel, 1, prize);

        [Command("create"), Alias("start")]
        public async Task Create(TimeSpan time, ITextChannel channel, [Remainder]string prize)
            => await Create(time, channel, 1, prize);

        [Command("create"), Alias("start")]
        public async Task Create(TimeSpan time, 
                                [Name("Winners")][PositiveNumberOnly]long winners,
                                [Remainder]string prize)
            => await Create(time, Context.TextChannel, winners, prize);

        [Command("create"), Priority(1), Alias("start")]
        public async Task Create(TimeSpan time, ITextChannel channel,
                                 [Name("Winners")][PositiveNumberOnly]long winners,
                                 [Remainder]string prize)
        {
            var msg = await ReplyAsync($":tada: The giveaway for the {prize} is starting in {channel.Mention}!");
            await Giveaways.Create(time, msg, winners, prize);
        }

        [Command("end")]
        public async Task End(IUserMessage message)
        {
            var giveaway = await Giveaways.Cancel(message);
            await giveaway.Run(message.Channel as ITextChannel);
        }

        [Command("reroll")]
        public async Task Reroll(IUserMessage message)
        {
            var users = await Giveaways.Reroll(message);
            await ReplyAsync($":tada: The new winner{(users.Count() > 1 ? "s" : "")} is " +
                $"{string.Join(", ", users.ToPropertyList(user => user.Mention))}! Congratulations!");
        }

        [Command("cancel")]
        public async Task Cancel(IUserMessage message)
        {
            await Giveaways.Cancel(message);
            await ReplyAsync(":boom: Alright, I guess we're not having a giveaway after all...");
        }
    }
}
