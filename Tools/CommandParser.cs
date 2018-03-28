using CWBDrone.Commands;
using CWBDrone.Commands.Preconditions;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;

namespace CWBDrone.Tools
{
    public static class CommandParser
    {
        public static readonly Regex RequireRoleIDs = new Regex(@"{{\.Require:(\d+)}}");
        public static readonly Regex RequireRoleNames = new Regex(@"{{\.Require:(.+)}}");
        public static readonly Regex NotRoleIDs = new Regex(@"{{\.Not:(\d+)}}");
        public static readonly Regex NotRoleNames = new Regex(@"{{\.Not:(.+)}}");
        public static readonly Regex RequireChannelIDs = new Regex(@"{{\.Require:#(\d+)}}");
        public static readonly Regex RequireChannelNames = new Regex(@"{{\.Require:#(.+)}}");
        public static readonly Regex NotChannelIDs = new Regex(@"{{\.Not:#(\d+)}}");
        public static readonly Regex NotChannelNames = new Regex(@"{{\.Not:#(.+)}}");

        public static readonly Regex Arguments = new Regex(@"\$(\d+)");

        public static readonly Regex DMUser = new Regex(@"{{.DM");

        public static PreconditionAttribute[] ParsePreconditions(ref string command)
        {
            var list = new List<PreconditionAttribute>();


            // Require Roles
            {
                var ids = new List<ulong>();
                var names = new List<string>();

                foreach (Match match in RequireRoleIDs.Matches(command))
                {
                    ids.Add(ulong.Parse(match.Groups[0].Value));
                    command = command.Replace(match.Value, "");
                }

                foreach (Match match in RequireRoleNames.Matches(command))
                {
                    names.Add(match.Groups[0].Value);
                    command = command.Replace(match.Value, "");
                }

                list.Add(new RequireRoleAttribute(ids.ToArray(), names.ToArray()));
            }

            // Not Roles
            {
                var ids = new List<ulong>();
                var names = new List<string>();

                foreach (Match match in NotRoleIDs.Matches(command))
                {
                    ids.Add(ulong.Parse(match.Groups[0].Value));
                    command = command.Replace(match.Value, "");
                }

                foreach (Match match in NotRoleNames.Matches(command))
                {
                    names.Add(match.Groups[0].Value);
                    command = command.Replace(match.Value, "");
                }

                list.Add(new NotRoleAttribute(ids.ToArray(), names.ToArray()));
            }

            // Require Channels
            {
                var ids = new List<ulong>();
                var names = new List<string>();

                foreach (Match match in RequireChannelIDs.Matches(command))
                {
                    ids.Add(ulong.Parse(match.Groups[0].Value));
                    command = command.Replace(match.Value, "");
                }

                foreach (Match match in RequireChannelNames.Matches(command))
                {
                    names.Add(match.Groups[0].Value);
                    command = command.Replace(match.Value, "");
                }

                list.Add(new RequireChannelAttribute(ids.ToArray(), names.ToArray()));
            }

            // Not Channels
            {
                var ids = new List<ulong>();
                var names = new List<string>();

                foreach (Match match in NotChannelIDs.Matches(command))
                {
                    ids.Add(ulong.Parse(match.Groups[0].Value));
                    command = command.Replace(match.Value, "");
                }

                foreach (Match match in NotChannelNames.Matches(command))
                {
                    names.Add(match.Groups[0].Value);
                    command = command.Replace(match.Value, "");
                }

                list.Add(new NotChannelAttribute(ids.ToArray(), names.ToArray()));
            }

            return list.ToArray();

        }

        public static string Format(string command, CWBContext context, params string[] parameters)
        {
            var cmd = VariableFormatting.FormatDateTime(context.ConfigGuild, command);
            cmd = cmd.Replace("{{.Prefix}}", context.Prefix);
            foreach (Match match in Arguments.Matches(cmd))
            {
                cmd = cmd.Replace(match.Value, parameters[int.Parse(match.Groups[0].Value)]);
            }

            return cmd;
        }

        public static async Task<string> Respond(string command, CWBContext context)
        {
            if (command.ContainsIgnoreCase("{{.Silent}}"))
            {
                return command.Replace("{{.Silent}}", "");
            }
            else if (command.ContainsIgnoreCase("{{.DM}}"))
            {
                command = command.Replace("{{.DM}}", "");
                await context.User.SendMessageAsync(command);
            }
            else if (command.ContainsIgnoreCase(".DM"))
            {

            }

            return command;
        }
    }
}
