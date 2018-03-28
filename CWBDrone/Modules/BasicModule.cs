using CWBDrone.Commands;
using CWBDrone.Tools;
using Discord;
using Discord.Commands;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWBDrone.Modules
{
    public class BasicModule : CWBModuleBase
    {
        public const ushort MaxAvatar = 2048;

        [Command("ping")]
        public async Task Ping()
        {
            var ping = $":ping_pong: Pong!\nMessage Round Trip: \tCalculating...\nDiscord Heartbeat: \t\t{Client.Latency} ms";
            var watch = Stopwatch.StartNew();
            var message = await ReplyAsync(ping);
            watch.Stop();
            ping = $":ping_pong: Pong!\nMessage Round Trip: \t{watch.Elapsed.TotalMilliseconds} ms" +
                $"\nDiscord Heartbeat: \t\t{Client.Latency} ms";
            await message.ModifyAsync(x => x.Content = ping);
        }

        [Command("userinfo")]
        public async Task Userinfo(IUser user = null)
        {
            user = user ?? Context.User;
            var embed = new EmbedBuilder();
            embed.AddField("ID", user.Id);

            embed.AddField("Username", user.Username);
            embed.AddField("Nickname", (user as IGuildUser)?.Nickname ?? "None");

            embed.AddField("Status", Enum.GetName(typeof(UserStatus), user.Status).ToLower());
            embed.AddField("Game", user.Activity != null ? Enum.GetName(typeof(ActivityType),
                user.Activity.Type) + " " + user.Activity.Name : "None");

            embed.AddField("Created", ConvertDateTime(user.CreatedAt));

            if (Context.Guild != null && user is IGuildUser guser)
            {
                embed.AddField("Joined", ConvertDateTime(guser.JoinedAt.Value));
                embed.AddField("Position", await FindPosition(guser));

                var roles = Context.Guild.Roles.Where(role =>
                {
                    return role.Id != Context.Guild.EveryoneRole.Id && guser.RoleIds.Contains(role.Id);
                });

                embed.AddField(roles.Count() > 1 ? "Roles" : "Role", string.Join(", ", roles.ToPropertyList(r => r.Name)));
                embed.AddField("Permissions", string.Join(", ", guser.GuildPermissions.GetNames()));
            }

            foreach (var field in embed.Fields)
            {
                field.IsInline = true;
            }

            embed.WithAuthor(user)
                .WithCurrentTimestamp()
                .WithThumbnailUrl(user.GetAvatarUrl())
                .WithColor((user as IGuildUser)?.GetEffectiveRoleColor() ?? Color.Default);

            await ReplyAsync("", embed: embed.Build());

            async Task<int> FindPosition(IGuildUser guildUser)
            {
                var users = await Context.Guild.GetUsersAsync();
                var list = new List<IUser>(users.OrderBy(u => u.JoinedAt));
                return list.FindIndex(u => u.Id == guildUser.Id);
            }
        }

        [Command("serverinfo")]
        [RequireContext(ContextType.Guild)]
        public async Task ServerInfo()
        {
            var guild = Context.Guild;
            var embed = new EmbedBuilder();

            embed.WithAuthor(guild.Name, guild.IconUrl)
                .WithCurrentTimestamp()
                .WithColor((await Context.Guild.GetUserAsync(Client.CurrentUser.Id)).GetEffectiveRoleColor());

            if (guild.IconUrl != null)
            {
                embed.WithThumbnailUrl(guild.IconUrl);
            }

            embed.AddField("ID", guild.Id);
            embed.AddField("Name", guild.Name);

            var owner = await guild.GetOwnerAsync();
            embed.AddField("Owner", owner.GetEffectiveName() + "#" + owner.DiscriminatorValue);

            embed.AddField("Region", guild.VoiceRegionId);
            embed.AddField("Created", ConvertDateTime(guild.CreatedAt));
            embed.AddField("Channels", (await guild.GetChannelsAsync()).Count());

            var users = await guild.DownloadGetUsersAsync();
            embed.AddField("Members", users.Count());
            embed.AddField("Humans", users.Count(gu => !gu.IsBot));
            embed.AddField("Bots", users.Count(gu => gu.IsBot));
            embed.AddField("Online", users.Count(gu =>
            {
                return !(gu.Status == UserStatus.Invisible || gu.Status == UserStatus.Offline);
            }));

            var roles = guild.Roles.Where(r => r.Id != guild.EveryoneRole.Id);
            embed.AddField("Roles", roles.Count());
            embed.AddField("Role List", string.Join(", ", roles.ToPropertyList(r => r.Name)));

            foreach (var field in embed.Fields)
            {
                field.IsInline = true;
            }

            await ReplyAsync("", embed: embed.Build());            
        }

        [Command("avatar")]
        public async Task Avatar(IUser user = null)
        {
            user = user ?? Context.User;
            await ReplyAsync("", embed: new EmbedBuilder
            {
                ImageUrl = user.GetAvatarUrl(ImageFormat.Png, MaxAvatar),
                Title = $"{user.GetEffectiveName()}'s avatar",
                Url = user.GetAvatarUrl(ImageFormat.Png, MaxAvatar),
                Color = (user as IGuildUser)?.GetEffectiveRoleColor() ?? Color.Default
            }.Build());
        }

        protected internal string ConvertDateTime(DateTimeOffset offset)
        {
            var zoned = ZonedDateTime.FromDateTimeOffset(offset);
            var tz = Context.ConfigGuild.TimeZone ?? DateTimeZoneProviders.Tzdb["America/Chicago"];
            var info = DateTimeFormatInfo.CurrentInfo.FullDateTimePattern;
            return zoned.WithZone(tz).ToString(info, null);
        }
    }
}

