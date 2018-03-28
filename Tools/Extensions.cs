using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CWBDrone.Tools
{
    public static class ParameterExtensions
    {
        public static bool Extends(this System.Reflection.ParameterInfo param, Type type)
            => param.ParameterType.Extends(type);
    }

    public static class PropertyExtensions
    {
        public static bool Extends(this PropertyInfo property, Type type)
            => property.PropertyType.Extends(type);
    }

    public static class TypeExtensions
    {
        public static bool Extends(this Type baseType, Type type)
            => baseType.GetInterfaces().Contains(type) || type.IsAssignableFrom(baseType);
    }

    public static class EnumerableExtensions
    {
        public static T Random<T>(this IEnumerable<T> enumerable)
            => enumerable.Random(new Random());

        public static T Random<T>(this IEnumerable<T> enumerable, Random random)
            => enumerable.ElementAt(random.Next(enumerable.Count()));

        public static IEnumerable<V> ToPropertyList<T, V>(this IEnumerable<T> enumerable, Func<T, V> func)
        {
            var list = new List<V>();
            foreach (var element in enumerable)
            {
                list.Add(func(element));
            }
            return list.AsEnumerable();
        }

    }

    public static class CollectionExtensions
    {
        public static void AddRange<T>(this ICollection<T> collect, params T[] elements)
            => collect.AddRange(collection: elements);

        public static void AddRange<T>(this ICollection<T> collect, IEnumerable<T> collection)
        {
            foreach (var item in collection)
            {
                collect.Add(item);
            }
        }
    }

    public static class StringExtensions
    {
        public static bool EqualsIgnoreCase(this string str, string value)
            => str.Equals(value, StringComparison.CurrentCultureIgnoreCase);

        public static bool ContainsIgnoreCase(this string str, string value)
            => str.IndexOf(value, StringComparison.CurrentCultureIgnoreCase) >= 0;

        public static bool StartsWithIgnoreCase(this string str, string value)
            => str.StartsWith(value, StringComparison.CurrentCultureIgnoreCase);
    }

    public static class ChannelExtension
    {
        public static async Task SendAndDeleteMessageAsync(this IMessageChannel channel, ulong millis, string text = "", bool isTTS = false, Embed embed = null, RequestOptions options = null)
        {
            var message = await channel.SendMessageAsync(text, isTTS, embed, options);
            await message.DeleteInAsync(millis, options);
        }

        public static Task<IUserMessage> SendMessageAsync(this IMessageChannel channel, string text = "", bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => channel.SendMessageAsync(text, isTTS, embed, options);
    }

    public static class TaskExtensions
    {
        public static async Task Delay(ulong delay)
        {
            ulong elapsed = 0;
            while (elapsed < delay)
            {
                var remaining = delay - elapsed;
                var next = remaining >= long.MaxValue / 10000 ? long.MaxValue / 10000 : (long)remaining;
                await Task.Delay(new TimeSpan(next * TimeSpan.TicksPerMillisecond));
                elapsed += (ulong)next * 10000;
            }
        }
    }

    public static class GuildPermissionsExtensions
    {
        public static string[] GetNames(this GuildPermissions permissions)
        {
            var array = new List<string>();
            if (permissions.AddReactions) array.Add("Add Reactions");
            if (permissions.Administrator) array.Add("Administrator");
            if (permissions.AttachFiles) array.Add("Attach Files");
            if (permissions.BanMembers) array.Add("Ban Members");
            if (permissions.ChangeNickname) array.Add("Change Nickname");
            if (permissions.Connect) array.Add("Connect");
            if (permissions.CreateInstantInvite) array.Add("Create Invite");
            if (permissions.DeafenMembers) array.Add("Deafen Members");
            if (permissions.EmbedLinks) array.Add("Embed Links");
            if (permissions.KickMembers) array.Add("Kick Members");
            if (permissions.ManageChannels) array.Add("Manage Channels");
            if (permissions.ManageEmojis) array.Add("Manage Emojis");
            if (permissions.ManageGuild) array.Add("Manage Guild");
            if (permissions.ManageMessages) array.Add("Manage Messages");
            if (permissions.ManageNicknames) array.Add("Manage Nicknames");
            if (permissions.ManageRoles) array.Add("Manage Roles");
            if (permissions.ManageWebhooks) array.Add("Manage Webhooks");
            if (permissions.MentionEveryone) array.Add("Mention Everyone");
            if (permissions.MoveMembers) array.Add("Move Members");
            if (permissions.MuteMembers) array.Add("Mute Members");
            if (permissions.ReadMessageHistory) array.Add("Read Message History");
            if (permissions.ReadMessages) array.Add("Read Messages");
            if (permissions.SendMessages) array.Add("Send Messages");
            if (permissions.SendTTSMessages) array.Add("Send TTS Messages");
            if (permissions.Speak) array.Add("Speak");
            if (permissions.UseExternalEmojis) array.Add("Use External Emojis");
            if (permissions.UseVAD) array.Add("Use Voice Activity");
            if (permissions.ViewAuditLog) array.Add("View Audit Log");


            return array.ToArray();

        }
    }

    public static class MessageExtensions
    {
        public static async Task DeleteInAsync(this IUserMessage message, ulong millis, RequestOptions options = null)
        {
            await TaskExtensions.Delay(millis);
            await message.DeleteAsync(options);
        }

        public static async Task<IReadOnlyCollection<IUser>> PaginateReactionUsersAsync(this IUserMessage message, IEmote emote, RequestOptions options = null)
        {
            var builder = new List<IUser>();
            ulong? lastUserID = null;
            for (int limit = 100; limit < message.Reactions.Count; limit += 100)
            {
                var users = await message.GetReactionUsersAsync(emote, 100, lastUserID);
                lastUserID = users.OrderByDescending(user => user.Id).First().Id;
                builder.AddRange(users);
            }
            return new ReadOnlyCollection<IUser>(builder);
        }

        public static bool HasPrefix(this IUserMessage message, string prefix, IUser user, ref int position)
            => message.HasPrefix(prefix, ref position) || message.HasMentionPrefix(user, ref position);

        public static bool HasPrefix(this IUserMessage message, string prefix, ref int position)
        {
            if (message.Content.StartsWithIgnoreCase(prefix))
            {
                position = prefix.Length;
                return true;
            }
            return false;
        }
    }

    public static class GuildExtensions
    {
        public static async Task<IReadOnlyCollection<IGuildUser>> DownloadGetUsersAsync(this IGuild guild)
        {
            await guild.DownloadUsersAsync();
            return await guild.GetUsersAsync();
        }
    }

    public static class UserExtensions
    {
        public static string GetEffectiveName(this IUser user)
            => (user as IGuildUser)?.Nickname ?? user.Username;
    }

    public static class GuildUserExtensions
    {
        public static Color GetEffectiveRoleColor(this IGuildUser user)
        {
            if (user.RoleIds.Any())
            {
                return user.Guild.Roles.OrderBy(role =>
                {
                    return role.Position;
                }).First(role =>
                {
                    return user.RoleIds.Contains(role.Id) 
                        && role.Color.RawValue != Color.Default.RawValue;
                }).Color;
            }
            else
            {
                return Color.Default;
            }
        }
    }

    public static class CommandInfoExtensions
    {
//      public static EmbedBuilder GetHelp(this CommandInfo command)
//          => GetHelp(command, ref new EmbedBuilder());

        public static void AppendHelp(this CommandInfo command, ref EmbedBuilder embed)
        {
           
        }
    }

    public static class ParameterInfoExtensions
    {
        public static string GetName(this Discord.Commands.ParameterInfo param)
            => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(param.Name);
    }

    public static class DiscordSocketExtensions
    {
        public static async Task<IReadOnlyCollection<IUser>> GetGuildUsersAsync(this IDiscordClient client)
        {
            var list = ImmutableArray.CreateBuilder<IUser>();

            foreach (var guild in await client.GetGuildsAsync())
            {
                list.AddRange(await guild.GetUsersAsync());
            }

            return list.ToImmutable();
        }
    }
}
