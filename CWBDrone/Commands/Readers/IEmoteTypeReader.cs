using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.Addons.EmojiTools;
using Discord;

namespace CWBDrone.Commands.Readers
{
    public class IEmoteTypeReader : TypeReader
    {
        public async override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            try
            {
                var emoji = EmojiExtensions.FromText(input);
                return await Task.FromResult(TypeReaderResult.FromSuccess(emoji));
            }
            catch
            {
                Emote emote;
                if (Emote.TryParse(input, out emote))
                {
                    return await Task.FromResult(TypeReaderResult.FromSuccess(emote));
                }

                return await Task.FromResult(TypeReaderResult.FromError(
					CommandError.ParseFailed, "IEmote not found."));
            }
        }
    }
}
