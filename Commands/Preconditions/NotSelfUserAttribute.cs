using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWBDrone.Commands.Preconditions
{
    public class NotSelfUserAttribute : ParameterPreconditionAttribute
    {

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
        {
            return context.User.Id == (value as IUser).Id ?
                Task.FromResult(PreconditionResult.FromError("You cannot tag yourself.")) :
                Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
