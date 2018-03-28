using CWBDrone.Commands;
using CWBDrone.Config;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWBDrone.Tools
{
    public static class VariableFormatting
    {
        public static async Task<string> FormatStatus(BaseSocketClient client, string str)
        {
            return str.Replace("{{.Guilds.Count}}", client.Guilds.LongCount().ToString())
                .Replace("{{.Users.Count}}", (await client.GetGuildUsersAsync()).LongCount().ToString())
                .Replace("{{.Uptime}}", "0");
        }

        public static string FormatUser(IUser user, string str)
        {
            return str.Replace("{{.User.Username}}", user.Username)
                .Replace("{{.User.Nickname}}", (user as IGuildUser)?.Nickname ?? "null")
                .Replace("{{.User.Name}}", (user as IGuildUser)?.Nickname ?? user.Username)
                .Replace("{{.User.ID}}", user.Id.ToString())
                .Replace("{{.User.Mention}}", user.Mention)
                .Replace("<@.User.ID>", user.Mention)
                .Replace("{{.User.Discriminator}}", user.Discriminator)
                .Replace("{{.User.Avatar.Url}}", user.GetAvatarUrl())
                .Replace("{{.User.Avatar.ID}}", user.AvatarId)
                .Replace("{{.User.Bot}}", (user.IsBot || user.IsWebhook) ? "true" : "false");
        }

        public static async Task<string> FormatGuild(IGuild guild, string str)
        {
            return str.Replace("{{.Guild.Name}}", guild.Name)
                    .Replace("{{.Guild.Icon.ID}}", guild.IconId)
                    .Replace("{{.Guild.Icon.Url}}", guild.IconUrl)
                    .Replace("{{.Guild.Region}}", guild.VoiceRegionId)
                    .Replace("{{.Guild.AfkChannelID}}", guild.AFKChannelId.HasValue ? guild.AFKChannelId.Value.ToString() : "null")
                    .Replace("{{.Guild.AfkTimeout", guild.AFKTimeout.ToString())
                    .Replace("{{.Guild.MemberCount}}", (await guild.DownloadGetUsersAsync()).LongCount().ToString())
                    .Replace("{{.Guild.VerificationLevel}}", Enum.GetName(typeof(VerificationLevel), guild.VerificationLevel));
        }

        public static string FormatMember(IGuildUser member, string str)
        {
            return str.Replace("{{.Member.Username}}", member.Username)
                    .Replace("{{.Member.Nickname}}", member.Nickname ?? "null")
                    .Replace("{{.Member.Name}}", member.Nickname ?? member.Username)
                    .Replace("{{.Member.ID}}", member.Id.ToString())
                    .Replace("{{.Member.Mention}}", member.Mention)
                    .Replace("<@.Member.ID>", member.Mention)
                    .Replace("{{.Member.Discriminator}}", member.Discriminator)
                    .Replace("{{.Member.Avatar.Url}}", member.GetAvatarUrl())
                    .Replace("{{.Member.Avatar.ID}}", member.AvatarId)
                    .Replace("{{.Member.Bot}}", (member.IsBot || member.IsWebhook) ? "true" : "false")
                    .Replace("{{.Member.JoinedAt}}", member.JoinedAt?.DateTime.ToLongDateString());
        }

        public static string FormatOwner(IGuildUser owner, string str)
        {
            return str.Replace("{{.Guild.Owner.Username}}", owner.Username)
                    .Replace("{{.Guild.Owner.Nickname}}", owner.Nickname ?? "null")
                    .Replace("{{.Guild.Owner.Name}}", owner.Nickname ?? owner.Username)
                    .Replace("{{.Guild.Owner.ID}}", owner.Id.ToString())
                    .Replace("{{.Guild.Owner.Mention}}", owner.Mention)
                    .Replace("<@.Guild.Owner.ID>", owner.Mention)
                    .Replace("{{.Guild.Owner.Discriminator}}", owner.Discriminator)
                    .Replace("{{.Guild.Owner.Avatar.Url}}", owner.GetAvatarUrl())
                    .Replace("{{.Guild.Owner.Avatar.ID}}", owner.AvatarId)
                    .Replace("{{.Guild.Owner.Bot}}", (owner.IsBot || owner.IsWebhook) ? "true" : "false");
        }

        public static string FormatChannel(IChannel channel, string str)
        {
            return str.Replace("{{.Channel.Name}}", channel.Name)
                    .Replace("{{.Channel.ID}}", channel.Id.ToString())
                    .Replace("<#.Channel.ID>", "<#" + channel.Id + ">")
                    .Replace("{{.Channel.Mention}}", "<#" + channel.Id + ">");
        }

        public static string FormatTextChannel(ITextChannel channel, IGuildUser user, string str)
        {
            return str.Replace("{{.Channel.Name}}", channel.Name)
                    .Replace("{{.Channel.ID}}", channel.Id.ToString())
                    .Replace("<#.Channel.ID>", channel.Id.ToString())
                    .Replace("{{.Channel.Mention}}", channel.Mention)
                    .Replace("{{.TextChannel.NSFW}}", channel.IsNsfw ? "true" : "false")
                    .Replace("{{.TextChannel.Topic}}", channel.Topic)
                    .Replace("{{.TextChannel.EmbedEnabled}}", user.GetPermissions(channel).EmbedLinks ? "true" : "false");
        }

        public static async Task<string> FormatMessage(CWBContext context, string str)
        {
            var value = str;
            if (value.ContainsIgnoreCase(".User"))
                value = FormatUser(context.User, value);

            if (value.ContainsIgnoreCase(".Guild"))
                value = await FormatGuild(context.Guild, value);

            if (value.ContainsIgnoreCase(".Guild.Owner"))
                value = FormatOwner(await context.Guild.GetOwnerAsync(), value);

            if (value.ContainsIgnoreCase(".Member"))
                value = FormatMember(context.GuildUser, value);

            if (value.ContainsIgnoreCase(".Channel"))
                value = FormatChannel(context.Channel, value);

            if (value.ContainsIgnoreCase(".TextChannel"))
                value = FormatTextChannel(context.TextChannel, await context.Guild.GetUserAsync(context.Bot.Socket.CurrentUser.Id), value);

            return value;
        }

        public static async Task<string> FormatJoin(IGuildUser user, string str)
        {
            var value = str;
            if (value.ContainsIgnoreCase(".User"))
                value = FormatUser(user, value);

            if (value.ContainsIgnoreCase(".Guild"))
                value = await FormatGuild(user.Guild, value);

            if (value.ContainsIgnoreCase(".Guild.Owner"))
                value = FormatOwner(await user.Guild.GetOwnerAsync(), value);

            if (value.ContainsIgnoreCase(".Member"))
                value = FormatMember(user, value);

            return value;
        }

        public static async Task<string> FormatLeave(IGuildUser user, string str)
        {
            var value = str;
            if (value.ContainsIgnoreCase(".User"))
                value = FormatUser(user, value);

            if (value.ContainsIgnoreCase(".Guild"))
                value = await FormatGuild(user.Guild, value);

            if (value.ContainsIgnoreCase(".Guild.Owner"))
                value = FormatOwner(await user.Guild.GetOwnerAsync(), value);

            if (value.ContainsIgnoreCase(".Member"))
                value = FormatMember(user, value);

            return value;
        }

        public static async Task<string> FormatBan(IUser user, IGuild guild, string str)
        {
            var value = str;
            
            if (value.ContainsIgnoreCase(".User"))
                value = FormatUser(user, value);

            if (value.ContainsIgnoreCase(".Guild"))
                value = await FormatGuild(guild, value);

            if (value.ContainsIgnoreCase(".Guild.Owner"))
                value = FormatOwner(await guild.GetOwnerAsync(), value);

            return value;
        }

        public static string FormatDateTime(ConfigGuild guild, string str)
        {
            var zdt = Instant.FromDateTimeUtc(DateTime.UtcNow).InZone(guild.TimeZone);
            var info = DateTimeFormatInfo.CurrentInfo;
            return str.Replace("{{.Date}}", zdt.ToString(info.LongDatePattern, null))
                .Replace("{{.DateTime}}", zdt.ToString(info.FullDateTimePattern, null))
                .Replace("{{.Time}}", zdt.ToString(info.LongTimePattern, null));
        }
    }

    public enum Month
    {

    }
}
