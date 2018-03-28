using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.Addons.EmojiTools;

namespace CWBDrone.Commands.Readers
{
    public class EmojiTypeReader : TypeReader
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
                return await Task.FromResult(TypeReaderResult.FromError(
                    CommandError.ParseFailed, "Not an emoji"));
            }
        }
    }
}
