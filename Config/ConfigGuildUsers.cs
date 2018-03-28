using System.Collections.Generic;
using System.Linq;

namespace CWBDrone.Config
{
    public class ConfigGuildUsers
    {
        public HashSet<ConfigGuildUser> Users { get; set; } = new HashSet<ConfigGuildUser>();

        public ConfigGuildUser this[ulong id]
        {
            get => GetUser(id);
            set => AddUser(id);
        }

        public ConfigGuildUser this[ConfigGuildUser user]
        {
            set => Users.Add(user);
        }

        public ConfigGuildUser GetUser(ulong id)
        {
            return Users.DefaultIfEmpty(AddUser(id)).First(user => user.ID == id);
        }

        public ConfigGuildUser AddUser(ulong id)
            => AddUser(id, out bool _);

        public ConfigGuildUser AddUser(ulong id, out bool success)
        {
            var user = new ConfigGuildUser
            {
                ID = id
            };

            success = Users.Add(user);
            return user;
        }
    }
}
