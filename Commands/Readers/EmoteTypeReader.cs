using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;

namespace CWBDrone.Commands.Readers
{
    public class EmoteTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            Emote emote;
            if (Emote.TryParse(input, out emote))
            {
                return Task.FromResult(TypeReaderResult.FromSuccess(emote));
            }

            return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Emote not found"));
        }
    }
}
