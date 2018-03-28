using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWBDrone.Commands.Preconditions
{
    public class BetaOwnerAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var drone = services.GetRequiredService<CWBDrone>();
            if (context.User.Id == (await drone.Rest.GetApplicationInfoAsync()).Owner.Id)
            {
                return PreconditionResult.FromSuccess();
            }
            else if (drone.LoadConfig.Beta)
            {
                if (context.User.Id == CWBDrone.HelpfulID)
                {
                    return PreconditionResult.FromSuccess();
                }

                return PreconditionResult.FromError($"{context.User.Username} is not the owner or Helpful since the bot is running in Beta mode");
            }

            return PreconditionResult.FromError($"{context.User.Username} is not the owner.");
        }
    }
}
