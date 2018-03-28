using CWBDrone.Config;
using CWBDrone.Tools;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace CWBDrone.Modules
{
    public class WGLBMessages
    {
        public DiscordRestClient Rest { get; set; }
        public DiscordSocketClient Client { get; set; }
        public CommandService Commands { get; set; }
        public IServiceProvider Services { get; set; }
        public CWBDrone Bot { get; set; }
        public Configuration Configuration { get; set; }

        public async Task WelcomeAsync(SocketGuildUser user)
        {
            var configGuild = Configuration.Guilds[user.Guild.Id];
            var channel = user.Guild.GetTextChannel(configGuild.WelcomeChannel);
            await channel.SendMessageAsync(await VariableFormatting.FormatJoin(user, configGuild.WelcomeMessage));
        }

        public async Task GreetingsAsync(SocketGuildUser user)
        {
            var configGuild = Configuration.Guilds[user.Guild.Id];
            var channel = user.Guild.GetTextChannel(configGuild.GreetingsChannel);
            var builder = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder()
                {
                    Name = user.Nickname ?? user.Username,
                    IconUrl = user.GetAvatarUrl()
                },
                Footer = new EmbedFooterBuilder
                {
                    Text = "User joined on",
                    IconUrl = user.GetAvatarUrl()
                },
                Timestamp = DateTimeOffset.UtcNow,
                Description = await VariableFormatting.FormatJoin(user, configGuild.GreetingsMessage),
                Color = configGuild.GreetingsColor.ToDiscordColor()
            };

            await Task.Run(async () =>
            {
                await BuildImageAsync(builder);
                await channel.SendMessageAsync(embed: builder.Build());
            });
        }

        public async Task<EmbedBuilder> BuildImageAsync(EmbedBuilder builder)
        {
            return await Task.FromResult(builder);
        }

        public async Task LeaveAsync(SocketGuildUser user)
        {
            var configGuild = Configuration.Guilds[user.Guild.Id];
            var channel = user.Guild.GetTextChannel(configGuild.LeaveChannel);
            await channel.SendMessageAsync(await VariableFormatting.FormatLeave(user, configGuild.LeaveMessage));
        }

        public async Task BanAsync(SocketUser user, SocketGuild guild)
        {
            var configGuild = Configuration.Guilds[guild.Id];
            var channel = guild.GetTextChannel(configGuild.BanChannel);
            await channel.SendMessageAsync(await VariableFormatting.FormatBan(user, guild, configGuild.BanMessage));
        }

        public RunMode GetRunMode()
        {
            return RunMode.Async;
        }
    }
}
