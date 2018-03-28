using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWBDrone.Commands.Preconditions
{
    public class GuildSpecificAttribute : PreconditionAttribute
    {
        public ulong ID { get; set; }

        public GuildSpecificAttribute(ulong id)
        {
            ID = id;
        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.Guild == null)
            {
                return PreconditionResult.FromError(CustomCommandsResult.FromError($"Command is not run in a guild!"));
            }

            if (context.Guild.Id == ID)
            {
                return await Task.FromResult(PreconditionResult.FromSuccess());
            }

            return PreconditionResult.FromError(CustomCommandsResult.FromError($"Command is not available in this guild!"));
        }
    }

    public class CustomCommandsResult : IResult
    {
        public CommandError? Error { get; }
        public string ErrorReason { get; }
        public bool IsSuccess => !Error.HasValue;

        protected CustomCommandsResult(CommandError? error, string errorReason)
        {
            Error = error;
            ErrorReason = errorReason;
        }

        public static CustomCommandsResult FromSuccess()
            => new CustomCommandsResult(null, null);

        public static CustomCommandsResult FromError(string reason)
            => new CustomCommandsResult(CommandError.UnknownCommand, reason);

        public static CustomCommandsResult FromError(IResult result)
            => new CustomCommandsResult(result.Error, result.ErrorReason);

        public override string ToString() => IsSuccess ? "Success" : $"{Error}: {ErrorReason}";
        private string DebuggerDisplay => IsSuccess ? "Success" : $"{Error}: {ErrorReason}";
    }
}
