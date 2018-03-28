using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWBDrone.Commands.Preconditions
{
    public class PositiveNumberOnlyAttribute : ParameterPreconditionAttribute
    {
        public static void Set(ParameterInfo info, ref PreconditionResult result)
        {
            result = PreconditionResult.FromError($"{info.Name} must be a positive, nonzero number.");
        }
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
        {
            PreconditionResult result = null;

            switch (value)
            {
                case bool b:
                    if (!b) Set(parameter, ref result);
                    break;
                case byte b:
                    if (b <= 0) Set(parameter, ref result);
                    break;
                case sbyte b:
                    if (b <= 0) Set(parameter, ref result);
                    break;
                case decimal d:
                    if (d <= 0) Set(parameter, ref result);
                    break;
                case double d:
                    if (d <= 0) Set(parameter, ref result);
                    break;
                case float f:
                    if (f <= 0) Set(parameter, ref result);
                    break;
                case int i:
                    if (i <= 0) Set(parameter, ref result);
                    break;
                case uint u:
                    if (u <= 0) Set(parameter, ref result);
                    break;
                case long l:
                    if (l <= 0) Set(parameter, ref result);
                    break;
                case ulong u:
                    if (u <= 0) Set(parameter, ref result);
                    break;
                case short s:
                    if (s <= 0) Set(parameter, ref result);
                    break;
                case ushort u:
                    if (u <= 0) Set(parameter, ref result);
                    break;
            }

            if (result == null || result.IsSuccess)
            {
                result = PreconditionResult.FromSuccess();
            }
            else
            {
                await context.Channel.SendMessageAsync(result.ErrorReason);
            }

            return result;
        }
    }
}
