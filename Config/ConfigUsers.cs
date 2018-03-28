using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace CWBDrone.Config
{
    public class ConfigUsers
    {
        public static readonly string Default = JsonConvert.SerializeObject(new ConfigUsers(), Formatting.Indented);

        public HashSet<ConfigUser> Users { get; set; } = new HashSet<ConfigUser>();

        public ConfigUser this[ulong id]
        {
            get => GetUser(id);
            set => AddUser(id);
        }

        public ConfigUser this[ConfigUser user]
        {
            set => Users.Add(user);
        }

        public ConfigUser GetUser(ulong id)
        {
            return Users.DefaultIfEmpty(AddUser(id)).First(user => user.ID == id);
        }

        public ConfigUser AddUser(ulong id)
            => AddUser(id, out bool _);

        public ConfigUser AddUser(ulong id, out bool success)
        {
            var user = new ConfigUser
            {
                ID = id
            };

            success = Users.Add(user);
            return user;
        }
    }

}
