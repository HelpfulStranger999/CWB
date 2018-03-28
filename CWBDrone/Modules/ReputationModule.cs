using CWBDrone.Commands;
using CWBDrone.Commands.Preconditions;
using CWBDrone.Services;
using CWBDrone.Tools;
using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWBDrone.Modules
{
    public class ReputationModule : CWBModuleBase
    {
        public ReputationService Reps { get; set; }

        [Command("rep")]
        public async Task Reputation([NotSelfUser]params IUser[] users)
        {
            if (!users.Any())
            {
                var builder = new StringBuilder($"You have {Reps.AvailableReputation(Context.User.Id)} reputation points available. ");
                if (Reps.AvailableReputation(Context.User.Id) > 0)
                {
                    var difference = Reps.NextReputation(Context.User.Id) - DateTimeOffset.Now;
                    builder.Append($"You will be able to give a point in {difference.Hours} hours, {difference.Minutes} minutes, and {difference.Seconds} seconds.");
                }

                await ReplyAsync(builder.ToString().Trim());
            }
            else
            {
                var embed = new EmbedBuilder
                {
                    Color = Context.GuildUser?.GetEffectiveRoleColor() ?? Color.Default
                };

                foreach (var user in users.Take(Reps.AvailableReputation(Context.User.Id)))
                {
                    var cuser = Configuration.Users[user.Id];
                    await Reps.RepUser(Configuration, cuser);
                    embed.AddField(user.GetEffectiveName(), $"{cuser.Reputation - 1} => {cuser.Reputation}", true);
                }
            }
        }
    }
}
