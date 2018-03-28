using Newtonsoft.Json;
using NodaTime;
using System.Collections.Generic;

namespace CWBDrone.Config
{
    public class ConfigGuild
    {
        public ulong ID { get; set; }
        public string Prefix { get; set; } = CWBDrone.Prefix;
        public ConfigGuildUsers GuildUsers { get; set; } = new ConfigGuildUsers();

        [JsonProperty("TimeZone")]
        public string TimezoneString
        {
            get => TimeZone.Id;
            set
            {
                TimeZone = DateTimeZoneProviders.Tzdb[value];
            }
        }

        [JsonIgnore]
        public DateTimeZone TimeZone { get; set; } = DateTimeZoneProviders.Tzdb["America/Chicago"];

        public ulong WelcomeChannel { get; set; } = 0;
        public string WelcomeMessage { get; set; } = "";

        public ulong GreetingsChannel { get; set; } = 0;
        public string GreetingsMessage { get; set; } = "";
        public string GreetingsBaseImage { get; set; } = "";
        public ConfigColor GreetingsColor { get; set; } = new ConfigColor
        {
            Red = 0,
            Blue = 0,
            Green = 0
        };

        public ulong LeaveChannel { get; set; } = 0;
        public string LeaveMessage { get; set; } = "";

        public ulong BanChannel { get; set; } = 0;
        public string BanMessage { get; set; } = "";

        public HashSet<ulong> AutoRoles { get; set; } = new HashSet<ulong>();
        public HashSet<ulong> BotAutoRoles { get; set; } = new HashSet<ulong>();

        public List<ConfigGiveaway> Giveaways { get; set; } = new List<ConfigGiveaway>();
        public Dictionary<string, string> CustomCommands { get; set; } = new Dictionary<string, string>();

        public override int GetHashCode() => ID.GetHashCode();
    }
}
