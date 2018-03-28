using CWBDrone.Commands;
using CWBDrone.Commands.Preconditions;
using CWBDrone.Config;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CWBDrone.Modules
{
    public class EvalModule : CWBModuleBase
    {

        public static string[] Namespaces;
        public static Assembly[] References;

        static EvalModule()
        {
            Namespaces = new string[]
            {
                "System",
                "System.IO",
                "System.Threading",
                "System.Threading.Tasks",
                "System.Collections.Generic",
                "System.Text",
                "System.Linq",
                "Discord.WebSocket",
                "Discord.Rest",
                "Discord.Commands",
                "Discord",
                "CWBDrone",
                "CWBDrone.Commands",
                "CWBDrone.Config",
                "CWBDrone.Modules",
                "CWBDrone.Services",
                "CWBDrone.Tools"
            };

            References = AppDomain.CurrentDomain.GetAssemblies().Where(a =>
            {
                return !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location);
            }).ToArray();
        }

        [Command("eval", RunMode = RunMode.Async)]
        [Alias("evaluate"), BetaOwner]
        public async Task Evaluate([Remainder]string codeText)
        {
            var variables = new ScriptVariables
            {
                Prefix = Context.Prefix,

                Rest = Rest,
                Client = Client,
                Commands = Commands,
                Context = Context,

                Bot = Bot,
                Config = Configuration,
                Services = Services,

                Guild = Context.Guild,
                ConfigGuild = Context.ConfigGuild,

                MessageChannel = Context.Channel,
                TextChannel = Context.TextChannel,

                User = Context.User,
                GuildUser = Context.GuildUser,
                ConfigUser = Configuration.Users[Context.User.Id],

                Message = Context.Message
            };

            var scriptOptions = ScriptOptions.Default
                                             .AddReferences(References)
                                             .AddImports(Namespaces);

            var code = codeText.Replace("```csharp", "")
                               .Replace("```", "");
            if (code.Contains("await"))
            {
                var builder = new StringBuilder();
                builder.Append("public async Task Main()\n{\n\t");
                builder.Append(code);
                builder.Append("\n}\n");
                builder.Append("Main().GetAwaiter().GetResult();\n");
                builder.Append("return \"Success!\";");
                code = builder.ToString();
            }

            var script = CSharpScript.Create(code, scriptOptions, typeof(ScriptVariables));
            var embed = await CreateBaseEmbedAsync(code);

            try
            {
                var result = await CSharpScript.EvaluateAsync(code,
                                   scriptOptions, variables, typeof(ScriptVariables));

                await ReplyAsync("", embed: await BuildEmbedAsync(ref embed, result));
            }
            catch (Exception e)
            {
                await ReplyAsync("", embed: await BuildEmbedAsync(ref embed, e));
            }
        }

        public async Task<EmbedBuilder> CreateBaseEmbedAsync(string code)
        {
            var embed = new EmbedBuilder
            {
                Color = new Color(241, 196, 15),
                Author = new EmbedAuthorBuilder
                {
                    Name = "CWBDrone Eval",
                    Url = null,
                    IconUrl = Client.CurrentUser.GetAvatarUrl()
                },
                Footer = new EmbedFooterBuilder
                {
                    Text = "Bot Developer or Owner Only",
                    IconUrl = Context.User.GetAvatarUrl()
                },
                Timestamp = DateTimeOffset.UtcNow
            };

            embed.AddField(new EmbedFieldBuilder
            {
                Name = "\ud83d\udce5 Input",
                Value = "```csharp\n" + code + "\n```",
                IsInline = false
            });

            return await Task.FromResult(embed);
        }

        public Task<Embed> BuildEmbedAsync(ref EmbedBuilder embed, object output)
        {
            if (output is Exception)
            {
                embed.AddField(new EmbedFieldBuilder
                {
                    Name = "\ud83d\udeab Error",
                    Value = "```csharp\n" + (output as Exception).Message + "\n```",
                    IsInline = false
                });
            }
            else
            {

                embed.AddField(new EmbedFieldBuilder
                {
                    Name = "\ud83d\udce4 Output",
                    Value = "```csharp\n" + output + "\n```",
                    IsInline = false
                });
            }

            return Task.FromResult(embed.Build());
        }
    }

    public class ScriptVariables
    {
        public string Prefix { get; set; }
        public DiscordRestClient Rest { get; set; }
        public DiscordSocketClient Client { get; set; }
        public CommandService Commands { get; set; }
        public CWBContext Context { get; set; }

        public CWBDrone Bot { get; set; }
        public Configuration Config { get; set; }
        public IServiceProvider Services { get; set; }

        public IGuild Guild { get; set; }
        public ConfigGuild ConfigGuild { get; set; }

        public IMessageChannel MessageChannel { get; set; }
        public ITextChannel TextChannel { get; set; }

        public IUser User { get; set; }
        public IGuildUser GuildUser { get; set; }
        public ConfigUser ConfigUser { get; set; }

        public IUserMessage Message { get; set; }
    }
}
