using CWBDrone.Services;
using CWBDrone.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;

namespace CWBDrone.Config
{
    public class Configuration : IService
    {
        public ImmutableDictionary<DatabaseType, string> Database { get; }
        public ConfigGuilds Guilds { get; protected set; }
        public ConfigUsers Users { get; protected set; }

        public Configuration(string database)
        {
            Database = new Dictionary<DatabaseType, string>
        {
            { DatabaseType.Guild, $@"{database}\guilds.json" },
            { DatabaseType.User,  $@"{database}\users.json"  }
        }.ToImmutableDictionary();
        }

        public async Task Connect()
        {
            if (File.Exists(Database[DatabaseType.Guild]))
            {
                Guilds = JsonConvert.DeserializeObject<ConfigGuilds>(File.ReadAllText(Database[DatabaseType.Guild]));
            }
            else
            {
                Guilds = new ConfigGuilds();
                using (var stream = File.CreateText(Database[DatabaseType.Guild]))
                {
                    await stream.WriteAsync(ConfigGuilds.Default);
                    await stream.FlushAsync();
                }
            }

            if (File.Exists(Database[DatabaseType.User]))
            {
                Users = JsonConvert.DeserializeObject<ConfigUsers>(File.ReadAllText(Database[DatabaseType.User]));
            }
            else
            {
                Users = new ConfigUsers();
                using (var stream = File.CreateText(Database[DatabaseType.User]))
                {
                    await stream.WriteAsync(ConfigUsers.Default);
                    await stream.FlushAsync();
                }
            }
        }

        public Task Refresh()
        {
            Guilds = null;
            Users = null;
            return Connect();
        }

        public async Task<ResultOperation> Write(DatabaseType dbType)
        {
            var db = Database[dbType];
            var data = "";
            switch (dbType)
            {
                case DatabaseType.Guild:
                    data = JsonConvert.SerializeObject(Guilds, Formatting.Indented);
                    break;
                case DatabaseType.User:
                    data = JsonConvert.SerializeObject(Users, Formatting.Indented);
                    break;
                default:
                    throw new InvalidOperationException($"DatabaseType {Enum.GetName(typeof(DatabaseType), dbType)} is not supported for write operations");
            }

            File.Delete(db);
            var file = File.CreateText(db);
            try
            {
                await file.WriteAsync(data);
                return ResultOperation.FromSuccess();
            }
            catch (Exception e)
            {
                return ResultOperation.FromError(e);
            }
            finally
            {
                file.Dispose();
            }
        }

        public async Task Disconnect()
        {
            var resultGuild = await Write(DatabaseType.Guild);
            if (resultGuild.IsError)
            {
                throw new Exception($"Error saving guild: {resultGuild.Exception.Message}", resultGuild.Exception);
            }

            var resultUser = Write(DatabaseType.User);
            if (resultGuild.IsError)
            {
                throw new Exception($"Error saving user: {resultUser.Exception.Message}", resultUser.Exception);
            }
        }

        public Task<bool> ReadyToDisconnect(CWBDrone bot)
        {
            return Task.FromResult(true);
        }
    }

    public enum DatabaseType
    {
        Guild = 0,
        User = 1
    }
}
