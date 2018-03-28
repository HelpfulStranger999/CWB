using Discord;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;

namespace CWBDrone
{
    public class LoadConfig
    {
        protected internal LoadConfig() { }

        public string Token { get; set; }
        public bool Beta { get; set; }
        public LogSeverity LogLevel { get; set; }
        public string Database { get; set; }
        public long Cooldown { get; set; }
        
    }

    public class LoadConfigBuilder
    {
        public string Token { get; set; } = null;
        public bool Beta { get; set; } = false;
        public LogSeverity LogLevel { get; set; } = LogSeverity.Info;
        public string Database { get; set; } = null;
        public long Cooldown { get; set; } = 86_400_000;

        public LoadConfig Build()
        {
            if (string.IsNullOrWhiteSpace(Token))
            {
                throw new ArgumentException($"{nameof(Token)} is null or whitespace - this must be properly set!");
            }

            if (string.IsNullOrWhiteSpace(Database))
            {
                throw new ArgumentNullException($"{nameof(Database)} is null or whitespace - this must be properly set!");
            }

            return new LoadConfig
            {
                Token = Token,
                Beta = Beta,
                LogLevel = LogLevel,
                Database = Database,
                Cooldown = Cooldown
            };
        }

        public static LoadConfig Load(string fileLocation)
        {
            string text = null;
            if (File.Exists(fileLocation))
            {
                text = File.ReadAllText(fileLocation);
            }
            else if (Uri.IsWellFormedUriString(fileLocation, UriKind.RelativeOrAbsolute))
            {
                using (var client = new WebClient())
                {
                    text = client.DownloadString(fileLocation);
                }
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new FileNotFoundException("Couldn't find configuration on local hard drive or on server!");
            }

            return JsonConvert.DeserializeObject<LoadConfig>(text);
        }
    }
}
