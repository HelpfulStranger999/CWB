using CWBDrone.Commands;
using CWBDrone.Commands.Preconditions;
using CWBDrone.Services;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace CWBDrone.Modules
{
    [BetaOwner]
    public class OwnerModule : CWBModuleBase
    {
        readonly static List<string> FileTypes = new List<string>
        {
            ".PNG", ".JPG", ".JPEG", ".GIF"
        };

        [Command("set-status")]
        public async Task SetStatus(SetStatus status)
        {
            await Client.SetStatusAsync((UserStatus)Enum.ToObject(
                typeof(UserStatus), (int)status));
            await ReplyAsync("Alright!");
        }

        [Command("list-games")]
        public async Task ListGames()
        {
            await ReplyAsync("These are the rotating games:\n" +
                string.Join("\n\t", Services.GetService<RotatingStatusService>().GetStatuses()));
        }

        [Command("set-rgame")]
        public async Task SetRotatingGame([Remainder]string games)
        {
            if (string.IsNullOrWhiteSpace(games)) { return; }
            var gameList = games.Split('|');
            var rotater = Services.GetService<RotatingStatusService>();
            rotater.SetStatus(gameList);

            await ReplyAsync($"Alright, {gameList.Count()} new status added.");
        }

        [Command("set-rspeed")]
        public Task SetRotatingSpeed(int speed)
            => SetRotatingSpeed(TimeSpan.FromMilliseconds(speed));

        [Command("set-rspeed")]
        public async Task SetRotatingSpeed(TimeSpan span)
        {
            if (span.TotalSeconds < 30)
            {
                await ReplyAsync("Speed must be greater than 30 seconds!");
            }
            else
            {
                Services.GetService<RotatingStatusService>().SetSpeed(span);
                await ReplyAsync("Alright, the speed has been set.");
            }
        }

        [Command("start-rotating")]
        public async Task StartRotating()
        {
            Services.GetService<RotatingStatusService>().Rotate();
            await ReplyAsync("Now rotating through statuses");
        }

        [Command("stop-rotating")]
        public async Task StopRotating()
        {
            Services.GetService<RotatingStatusService>().Stop();
            await ReplyAsync("Stopping rotationery");
        }

        [Command("set-game")]
        public async Task SetGame([Remainder]string game)
        {
            if (string.IsNullOrWhiteSpace(game)) { return; }
            await Client.SetGameAsync(game, null, Client.CurrentUser.Activity?.Type ?? ActivityType.Playing);
            await ReplyAsync("Alright!");
        }

        [Command("set-activity")]
        public async Task SetActivity(SetActivity activity)
        {
            var curr = Client.Activity;
            await Client.SetActivityAsync(new Game(curr.Name, (ActivityType)Enum
                        .ToObject(typeof(ActivityType), (int)activity)));
            await ReplyAsync("Alright!");
        }

        [Command("set-avatar")]
        public async Task SetAvatar()
        {
            var attachment = Context.Message.Attachments.FirstOrDefault(file =>
            {
                return FileTypes.Contains(Path.GetExtension(file.Filename).ToUpperInvariant());
            });

            if (attachment == null) { return; }

            using (var client = new WebClient())
            {
                using (var stream = new MemoryStream(client.DownloadData(attachment.ProxyUrl)))
                {
                    try
                    {
                        await Client.CurrentUser.ModifyAsync(x =>
                        {
                            x.Avatar = new Image(stream);
                        });

                        await ReplyAsync("Alright!");
                    }
                    catch
                    {
                        await ReplyAsync("Ratelimited - try again later");
                    }
                }
            }
        }

        [Command("set-username")]
        public async Task SetUsername([Remainder]string username)
        {
            if (string.IsNullOrWhiteSpace(username)) { return; }
            try
            {
                await Client.CurrentUser.ModifyAsync(x => x.Username = username);
                await ReplyAsync("Alright!");
            }
            catch
            {
                await ReplyAsync("Ratelimited - try again later");
            }
        }

        [Command("forcestop")]
        public async Task HardStop()
        {
            await ReplyAsync("Shutting down now...");
            await Task.Factory.StartNew(async () =>
            {
                await Bot.StopAsync(false);
                await Bot.UnloadAsync();
                Environment.Exit(0);
            }, TaskCreationOptions.LongRunning);
        }

        [Command("forcerestart")]
        public async Task HardRestart()
        {
            await ReplyAsync("Restarting now...");
            await Task.Factory.StartNew(async () => { await Bot.RestartAsync(false); },
                                        TaskCreationOptions.LongRunning);
        }

        [Command("stop")]
        public async Task Stop()
        {
            await ReplyAsync("Shutting down soon...");
            await Task.Factory.StartNew(async () =>
            {
                await Bot.StopAsync();
                await Bot.UnloadAsync();
                Environment.Exit(0);
            }, TaskCreationOptions.LongRunning);
        }

        [Command("restart")]
        public async Task Restart()
        {
            await ReplyAsync("Restarting soon...");
            await Task.Factory.StartNew(async () => { await Bot.RestartAsync(); },
                                        TaskCreationOptions.LongRunning);
        }
    }

    public enum SetStatus
    {
        Idle = UserStatus.Idle,
        DoNotDisturb = UserStatus.DoNotDisturb,
        DND = UserStatus.DoNotDisturb,
        Offline = UserStatus.Online,
        Online = UserStatus.Online,
        Invisible = UserStatus.Invisible,
        AFK = UserStatus.AFK
    }

    public enum SetActivity
    {
        Listening = ActivityType.Listening,
        Watching = ActivityType.Watching,
        Playing = ActivityType.Playing
    }
}
