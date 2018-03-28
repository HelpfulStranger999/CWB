using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace CWBDrone.Commands.Preconditions
{
    public class RequireRoleAttribute : RequireContextAttribute
    {
        public ulong[] RoleIDs { get; set; }
        public string[] RoleNames { get; set; }


        public RequireRoleAttribute(params ulong[] roleIDs) : this(roleIDs, new string[0]) { }
        public RequireRoleAttribute(params string[] roleNames) : this(new ulong[0], roleNames) { }

        public RequireRoleAttribute(ulong[] roleIDs, string[] roleNames) : base(ContextType.Guild)
        {
            RoleIDs = roleIDs;
            RoleNames = roleNames;
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var roles = new List<ulong>();
            foreach (var role in from r in context.Guild.Roles
                                 where RoleIDs.Contains(r.Id) || RoleNames.Contains(r.Name)
                                 select r)
            {
                roles.Add(role.Id);
            }

            return (context.User as IGuildUser).RoleIds.Intersect(roles).Any() ?
                Task.FromResult(PreconditionResult.FromSuccess()) :
                Task.FromResult(PreconditionResult.FromError($"User does not have required role!"));
        }
    }
}
