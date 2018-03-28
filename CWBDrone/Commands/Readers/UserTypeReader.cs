using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Discord;
using System.Globalization;

namespace CWBDrone.Commands.Readers
{
    public class UserTypeReader : TypeReader
    {
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var bot = services.GetService<DiscordSocketClient>();
            var rest = services.GetService<DiscordRestClient>();
            var results = new Dictionary<ulong, TypeReaderValue>();
            var users = new List<IUser>();

            foreach (var guild in bot.Guilds)
            {
                users.AddRange(guild.Users);
            }


            //By Mention
            if (MentionUtils.TryParseUser(input, out ulong id))
            {
                return TypeReaderResult.FromSuccess(await rest.GetUserAsync(id));
            }

            //By Id
            if (ulong.TryParse(input, NumberStyles.None, CultureInfo.InvariantCulture, out id))
            {
                return TypeReaderResult.FromSuccess((IUser)rest.GetUserAsync(id));
            }

            //By Username + Discriminator
            var index = input.LastIndexOf('#');
            if (index >= 0)
            {
                var username = input.Substring(0, index);
                if (ushort.TryParse(input.Substring(index + 1), out ushort discrim))
                {

                    if (users.Any(u => u.Username.Equals(input, StringComparison.CurrentCultureIgnoreCase)
                                 && u.DiscriminatorValue == discrim))
                    {
                        return TypeReaderResult.FromSuccess(users.First(u => u.Username
                                               .Equals(input, StringComparison.CurrentCultureIgnoreCase)
                                               && u.DiscriminatorValue == discrim));
                    }

                    if (users.Any(u => (u as IGuildUser)?.Nickname.Equals(input,
                                          StringComparison.CurrentCultureIgnoreCase) ?? false
                                  && u.DiscriminatorValue == discrim))
                    {
                        return TypeReaderResult.FromSuccess(users.First(u => (u as IGuildUser)?
                                               .Nickname.Equals(input, StringComparison
                                               .CurrentCultureIgnoreCase) ?? false
                                                && u.DiscriminatorValue == discrim));
                    }
                }
            }

            //By Username/Nickname
            {
                if (users.Any(u => u.Username.Equals(input, StringComparison.CurrentCultureIgnoreCase)))
                {
                    return TypeReaderResult.FromSuccess(users.First(u => u.Username
                                           .Equals(input, StringComparison.CurrentCultureIgnoreCase)));
                }

                if (users.Any(u => (u as IGuildUser)?.Nickname.Equals(input,
                                      StringComparison.CurrentCultureIgnoreCase) ?? false))
                {
                    return TypeReaderResult.FromSuccess(users.First(u => (u as IGuildUser)?
                                           .Nickname.Equals(input, StringComparison
                                           .CurrentCultureIgnoreCase) ?? false));
                }
            }

            return await Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, "User not found."));
        }
    }
}
