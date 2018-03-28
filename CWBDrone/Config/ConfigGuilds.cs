using Newtonsoft.Json;
using System.Collections.Generic;

namespace CWBDrone.Config
{
    public class ConfigGuilds
    {
        public static readonly string Default = JsonConvert.SerializeObject(new ConfigGuilds(), Formatting.Indented);

        public HashSet<ConfigGuild> Servers
        {
            get => new HashSet<ConfigGuild>(Guilds.Values);
            set
            {
                foreach (var guild in value)
                {
                    Guilds.Add(guild.ID, guild);
                }
            }
        }

        [JsonIgnore]
        public Dictionary<ulong, ConfigGuild> Guilds { get; set; } = new Dictionary<ulong, ConfigGuild>();

        public ConfigGuild this[ulong id]
        {
            get => GetGuild(id);
            set => AddGuild(id);
        }

        public ConfigGuild this[ConfigGuild guild]
        {
            set => Guilds.Add(guild.ID, guild);
        }

        public ConfigGuild GetGuild(ulong id)
        {
            return Guilds[id] ?? null;
        }

        public ConfigGuild AddGuild(ulong id, string prefix = CWBDrone.Prefix)
        {
            var guild = new ConfigGuild
            {
                ID = id,
                Prefix = prefix
            };

            Servers.Add(guild);
            Guilds.Add(id, guild);
            return guild;
        }

        public IEnumerator<ConfigGuild> GetEnumerator()
        {
            return Guilds.Values.GetEnumerator();
        }

        public bool Contains(ulong id)
        {
            return Guilds.ContainsKey(id);
        }
    }
}
