using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CWBDrone.Commands.Readers
{
    public class TimeSpanTypeReader : TypeReader
    {
        public const long FromWeek        = 604_800_000;
        public const long FromDay         = 86_400_000;
        public const long FromHour        = 3_600_000;
        public const long FromMinute      = 60_000;
        public const long FromSecond      = 1_000;
        public const long FromMilliSecond = 1;

        public static readonly Regex Week        = new Regex(@"(\d+)[w]", RegexOptions.IgnoreCase);
        public static readonly Regex Day         = new Regex(@"(\d+)[d]", RegexOptions.IgnoreCase);
        public static readonly Regex Hour        = new Regex(@"(\d+)[h]", RegexOptions.IgnoreCase);
        public static readonly Regex Minute      = new Regex(@"(\d+)[m](?!s)", RegexOptions.IgnoreCase);
        public static readonly Regex Second      = new Regex(@"(\d+)[s]", RegexOptions.IgnoreCase);
        public static readonly Regex MilliSecond = new Regex(@"(\d+)(ms)", RegexOptions.IgnoreCase);

        public static readonly ImmutableDictionary<long, Regex> Parsers = new Dictionary<long, Regex>
        {
            { FromMilliSecond, MilliSecond },
            { FromSecond, Second },
            { FromMinute, Minute },
            { FromHour, Hour },
            { FromDay, Day },
            { FromWeek, Week},
        }.ToImmutableDictionary();


        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var millis = 0L;
            TypeReaderResult result;

            foreach (var pair in Parsers)
            {
                foreach (Match match in pair.Value.Matches(input))
                {
                    input = input.Replace(match.Value, "");
                    millis += pair.Key * long.Parse(match.Groups[1].Value);
                }
            }

            if (millis == 0)
            {
                if (TimeSpan.TryParse(input, out TimeSpan time))
                {
                    result = TypeReaderResult.FromSuccess(time);
                }
                else
                {
                    result = TypeReaderResult.FromError(CommandError.ParseFailed, 
                        $"Failed to parse timespan from input, or input was 0 milliseconds");
                }
            }
            else
            {
                result = TypeReaderResult.FromSuccess(TimeSpan.FromMilliseconds(millis));
            }

            return Task.FromResult(result);
        }
    }
}
