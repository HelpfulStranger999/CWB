using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CWBDrone.Config
{
    public class ConfigGuildUser
    {
        public ulong ID { get; set; }

        public ulong Currency { get; set; } = 0;
        public ulong Dollars { get; set; } = 0;
        public ulong Experience { get; set; } = 0;
        public HashSet<ulong> Roles { get; set; } = new HashSet<ulong>();

        public long MuteTimestamp
        {
            get => MuteTime.ToUnixTimeMilliseconds();
            set => MuteTime = DateTimeOffset.FromUnixTimeMilliseconds(value);
        }

        [JsonIgnore]
        public DateTimeOffset MuteTime { get; set; } = DateTimeOffset.Now.AddMinutes(-1);

        public override int GetHashCode() => ID.GetHashCode();
    }
}
