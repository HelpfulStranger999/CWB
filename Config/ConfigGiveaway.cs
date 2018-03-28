using CWBDrone.Tools;
using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CWBDrone.Config
{
    public class ConfigGiveaway
    {
        public static readonly Emoji Giveaway = new Emoji("🎉");
        public ulong MessageID { get; set; }
        public string Prize { get; set; }
        public long WinnerCount { get; set; }

        public long GiveawayDuration
        {
            get => Duration.ToUnixTimeMilliseconds();
            set => Duration = DateTimeOffset.Now + TimeSpan.FromMilliseconds(value);
        }

        [JsonIgnore]
        public DateTimeOffset Duration { get; set; }

        [JsonIgnore]
        internal Timer RunTimer { get; private set; }

        internal void SetTimer(ITextChannel channel, TimeSpan span)
        {
            RunTimer = new Timer(async _ =>
            {
                await Run(channel);
                await channel.SendMessageAsync("");
            }, null, span, Timeout.InfiniteTimeSpan);
        }

        public override int GetHashCode()
            => MessageID.GetHashCode();

        public async Task<IUser[]> Run(ITextChannel channel)
        {
            var message = await channel.GetMessageAsync(MessageID) as IUserMessage
                ?? throw new ArgumentNullException(nameof(channel));

            var users = (await message.PaginateReactionUsersAsync(Giveaway)).ToArray();

            var rng = new Random();
            var winners = new IUser[WinnerCount];

            for (int i = 0; i < WinnerCount; i++)
            {
                var winner = rng.Next(users.Count(u => u != null));
                winners[i] = users[i];
                users[i] = null;
            }

            await message.ModifyAsync(x =>
            {
                x.Embed = message.Embeds.First().ToEmbedBuilder()
                        .WithFooter("Ended at")
                        .WithDescription($"Winner{(WinnerCount == 1 ? "" : "s")}\n" +
                            string.Join("\n", winners.ToPropertyList(user => user.Mention)))
                        .Build();
            });

            return winners;
        }
    }
}
