using CWBDrone.Commands;
using CWBDrone.Commands.Attributes;
using CWBDrone.Commands.Preconditions;
using CWBDrone.Commands.Readers;
using CWBDrone.Config;
using CWBDrone.Modules;
using CWBDrone.Services;
using CWBDrone.Tools;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CWBDrone
{
    public class CWBDrone
    {
        // Constants
        public const string Prefix = "-";
        public const ulong BubbaID = 130895717257445376;
        public const ulong HelpfulID = 296763206540460033;

        // Constructors
        public CWBDrone(string location) : this(LoadConfigBuilder.Load(location)) { }
        public CWBDrone(LoadConfig config)
        {
            LoadConfig = config;
        }

        // Fields
        protected internal IEnumerable<Type> IListeners = from type in Assembly.GetEntryAssembly().GetTypes()
                                                          where typeof(IListener).IsAssignableFrom(type) && !type.IsAbstract
                                                          select type;

        // Events
        public delegate Task _ConsoleInput(string input);
        public event _ConsoleInput OnConsoleInput;

        // Properties
        public LoadConfig LoadConfig { get; }
        public Dictionary<Type, bool> DisconnectList { get; } = new Dictionary<Type, bool>();
        public DiscordRestClient Rest { get; protected set; }
        public DiscordSocketClient Socket { get; protected set; }
        public CommandService Commands { get; protected set; }
        public IServiceProvider Services { get; protected set; }
        protected internal List<IService> IServices { get; protected set; } = new List<IService>();
        public Configuration Configuration { get; protected set; }

        // Base Methods
        public async Task LoadAsync()
        {
            Rest = new DiscordRestClient(new DiscordRestConfig
            {
                DefaultRetryMode = RetryMode.AlwaysRetry,
                LogLevel = LoadConfig.LogLevel
            });

            Socket = new DiscordSocketClient(new DiscordSocketConfig
            {
                DefaultRetryMode = RetryMode.AlwaysRetry,
                LogLevel = LoadConfig.LogLevel
            });

            Commands = new CommandService(new CommandServiceConfig
            {
                IgnoreExtraArgs = true,
                DefaultRunMode = RunMode.Sync,
                CaseSensitiveCommands = false,
                LogLevel = LoadConfig.LogLevel
            });

            var collection = new ServiceCollection();
            collection.AddSingleton(Rest)
                      .AddSingleton(Socket)
                      .AddSingleton(Commands)
                      .AddSingleton(this);

            Configuration = new Configuration(LoadConfig.Database);
            collection.AddSingleton(Configuration);

            IServices.AddRange(new ReputationService(LoadConfig),
                new GiveawayService(Configuration),
                new RotatingStatusService(Socket),
                new ReputationService(LoadConfig));

            foreach (var service in IServices)
            {
                DisconnectList[service.GetType()] = false;
                collection.AddSingleton(service.GetType(), service);
            }

            Services = collection.BuildServiceProvider();

            Commands.AddTypeReader(typeof(IUser), new UserTypeReader());
            Commands.AddTypeReader(typeof(TimeSpan), new TimeSpanTypeReader());
            Commands.AddTypeReader(typeof(IEmote), new IEmoteTypeReader());
            Commands.AddTypeReader(typeof(Emoji), new EmojiTypeReader());
            Commands.AddTypeReader(typeof(Emote), new EmoteTypeReader());
            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), Services);

            Socket.Ready += Ready;

            Socket.MessageReceived += HandleCommandsAsync;
            Socket.MessageReceived += HandleMessagesAsync;

            Rest.Log += Log;
            Socket.Log += Log;
            Commands.Log += Log;

            var wglb = DependencyInjection.CreateInjected<WGLBMessages>(Services);
            Socket.UserJoined += wglb.GreetingsAsync;
            Socket.UserJoined += wglb.WelcomeAsync;
            Socket.UserLeft += wglb.LeaveAsync;
            Socket.UserBanned += wglb.BanAsync;

            var autoroles = DependencyInjection.CreateInjected<AutoRoleModule>(Services);
            Socket.UserJoined += autoroles.AutoRoleAsync;

            OnConsoleInput += ConsoleCommand;
            AppDomain.CurrentDomain.ProcessExit += async (sender, eventArgs) =>
            {
                if (Socket.ConnectionState == ConnectionState.Connected)
                {
                    await StopAsync(false);
                    await UnloadAsync();
                }
            };
        }

        protected internal async Task CustomCommandAsync()
        {
            foreach (var guild in Configuration.Guilds)
            {
                await Commands.CreateModuleAsync("", module =>
                {
                    module.AddPrecondition(new GuildSpecificAttribute(guild.ID));
                    foreach (var command in guild.CustomCommands)
                    {
                        var value = command.Value;
                        module.AddCommand(command.Key, async (context, parameters, services, info) =>
                        {
                            var cmd = await VariableFormatting.FormatMessage(context as CWBContext, value);
                            CommandParser.Format(cmd, context as CWBContext, (parameters[0] as string)?.Split(' '));
                            await CommandParser.Respond(cmd, context as CWBContext);
                        }, builder =>
                        {
                            foreach (var pc in CommandParser.ParsePreconditions(ref value))
                            {
                                builder.AddPrecondition(pc);
                            }

                            builder.AddAttributes(new CustomCommandAttribute());
                            builder.AddParameter("objects", typeof(string), param =>
                            {
                                param.IsOptional = true;
                                param.IsRemainder = true;
                            });
                        });
                    }
                });
            }
        }

        public async Task StartAsync()
        {
            await Configuration.Connect();

            await Rest.LoginAsync(TokenType.Bot, LoadConfig.Token);

            await Socket.LoginAsync(TokenType.Bot, LoadConfig.Token);
            await Socket.StartAsync();
        }

        public async Task StopAsync(bool graceful = true)
        {
            if (graceful)
            {
                Parallel.ForEach(IServices, async service =>
                {
                    DisconnectList[service.GetType()] = await service.ReadyToDisconnect(this);
                });

                SpinWait.SpinUntil(() => DisconnectList.All(pair => pair.Value));
            }

            if (IServices.Count > 0)
            {
                var disconnects = new Task[IServices.Count];
                for (var i = 0; i < disconnects.Count(); i++)
                {
                    disconnects[i] = IServices[i].Disconnect();
                }

                Task.WaitAny(Task.WhenAll(disconnects), Task.Delay(5 * 60 * 1000));
            }

            await Rest.LogoutAsync();
            await Socket.LogoutAsync();
            await Socket.StopAsync();
            await Configuration.Disconnect();
        }

        public async Task UnloadAsync()
        {
            for (var i = 0; i < Commands.Modules.Count(); i++)
            {
                await Commands.RemoveModuleAsync(Commands.Modules.ElementAt(i));
            }

            Commands = null;
            Services = null;

            IServices.Clear();
            IServices = null;

            Rest = null;
            Socket = null;
            Configuration = null;
        }

        public async Task RestartAsync(bool graceful = true)
        {
            await StopAsync(graceful);
            await StartAsync();
        }

        public async Task Ready()
        {
            var unloadedGuilds = from guild in Socket.Guilds
                                 where !Configuration.Guilds.Contains(guild.Id)
                                 select guild;

            foreach (var guild in unloadedGuilds)
            {
                Configuration.Guilds.AddGuild(guild.Id);
            }

            if (unloadedGuilds.LongCount() > 0)
            {
                await Configuration.Write(DatabaseType.User);
            }
        }

        public async Task HandleCommandsAsync(SocketMessage msg)
        {
            if (msg.Author.IsWebhook || msg.Author.Id == Socket.CurrentUser.Id) { return; }

            if (msg is SocketUserMessage message)
            {
                var pos = 0;
                var prefix = (message.Channel is ITextChannel) ? Configuration.Guilds.GetGuild(
                    (message.Channel as ITextChannel).GuildId).Prefix : Prefix;

                if (message.HasPrefix(prefix, Socket.CurrentUser, ref pos))
                {
                    var context = new CWBContext(this, message);
                    var result = await Commands.ExecuteAsync(context, message.Content.Substring(pos), Services);
                    if (!result.IsSuccess)
                    {
                        await HandleCommandError(result);
                    }
                }
            }
        }

        public async Task HandleMessagesAsync(SocketMessage msg)
        {
            if (msg is SocketUserMessage message)
            {
                var _ = 0;
                var prefix = (message.Channel is ITextChannel) ? Configuration.Guilds.GetGuild(
                        (message.Channel as ITextChannel).GuildId).Prefix : Prefix;

                if(message.HasPrefix(prefix, Socket.CurrentUser, ref _)) { return; }

                var context = new CWBContext(this, message);
                foreach (var type in IListeners)
                {
                    var listener = DependencyInjection.CreateInjected<IListener>(type, Services, context);
                    if (listener.GetRunMode() == RunMode.Async)
                    {
                        await Task.Run(async () =>
                        {
                            try
                            {
                                await listener.ExecuteAsync();
                            }
                            catch (Exception e)
                            {
                                await HandleCommandError(ExecuteResult.FromError(e));
                            }
                        });
                    }
                    else
                    {
                        try
                        {
                            await listener.ExecuteAsync();
                        }
                        catch (Exception e)
                        {
                            await HandleCommandError(ExecuteResult.FromError(e));
                        }
                    }
                }
            }
        }

        public async Task HandleCommandError(IResult result)
        {
            switch (result.Error.Value)
            {
                case CommandError.BadArgCount:
                case CommandError.UnknownCommand:
                case CommandError.UnmetPrecondition:
                    // Fail silently.
                    break;
                case CommandError.ObjectNotFound:
                case CommandError.Exception:
                case CommandError.Unsuccessful:
                    // Forward to console for my review
                    await Log(new LogMessage(LogSeverity.Error, null, $"Error:\t{result.ErrorReason}"));
                    break;
                case CommandError.MultipleMatches:
                    // Forward to console for my review
                    await Log(new LogMessage(LogSeverity.Warning, null, $"Warning: Multiple matches found - {result.ErrorReason}"));
                    break;
                case CommandError.ParseFailed:
                    // TODO Send Help
                    break;
                default:
                    break;
            }
        }

        public Task Log(LogMessage message)
        {
            switch (message.Severity)
            {
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Error:
                case LogSeverity.Critical:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
            }

            Console.WriteLine($"{DateTime.Now.ToLongTimeString()} {nameof(CWBDrone)}:{"\t" + message.Source ?? ""} \t{message.Message}");
            Console.ResetColor();

            return Task.CompletedTask;
        }

        public async Task StartConsole()
        {
            var line = Console.ReadLine();
            await Task.Run(async () => await OnConsoleInput(line));
            await StartConsole();
        }

        public async Task ConsoleCommand(string line)
        {
            if (line.EqualsIgnoreCase("stop"))
            {
                await StopAsync(true);
            }
            else if (line.EqualsIgnoreCase("restart"))
            {
                await RestartAsync(true);
            }
            else if (line.EqualsIgnoreCase("force stop"))
            {
                Task.WaitAll(StopAsync(false));
                Task.WaitAll(UnloadAsync());
                Environment.Exit(0);
            }
            else if (line.EqualsIgnoreCase("force restart"))
            {
                await RestartAsync(false);
            }
        }

    }
}
