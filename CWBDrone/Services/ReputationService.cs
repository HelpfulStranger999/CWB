using CWBDrone.Config;
using Discord;
using System;
using System.Linq;
using System.Threading.Tasks;
using Reputations = System.Collections.Generic.Dictionary<ulong, System.DateTimeOffset[]>;

namespace CWBDrone.Services
{
    public class ReputationService : DefaultService
    {
        public DateTimeOffset[] BaseTimes
        {
            get => new DateTimeOffset[]
                {
                    DateTimeOffset.Now,
                    DateTimeOffset.Now,
                    DateTimeOffset.Now
                };
        }

        public long Cooldown { get; }

        public ReputationService(LoadConfig config) : this(config.Cooldown) { }
        public ReputationService(long cooldown) => Cooldown = cooldown;

        protected Reputations Reputations { get; } = new Reputations();

        public bool CanRep(IUser user) => CanRep(user.Id);
        public bool CanRep(ulong id) => NextReputation(id) <= DateTimeOffset.Now;

        public DateTimeOffset NextReputation(IUser user) => NextReputation(user.Id);
        public DateTimeOffset NextReputation(ulong id)
        {
            return RegisterUser(id).OrderBy(time => time.ToUnixTimeMilliseconds()).First();
        }

        protected internal DateTimeOffset[] RegisterUser(ulong id)
        {
            if (Reputations.ContainsKey(id))
            {
                return Reputations[id];
            }

            Reputations.Add(id, new DateTimeOffset[]
            {
                DateTimeOffset.Now,
                DateTimeOffset.Now,
                DateTimeOffset.Now
            });

            return Reputations[id];
        }

        protected internal void Insert(ulong id, DateTimeOffset reputation)
        {
            var reps = Reputations[id];
            for (var i = 0; i < reps.Count(); i++)
            {
                var rep = reps[i];
                if (reputation >= rep)
                {
                    Reputations[id][i] = reputation;
                    return;
                }
            }
        }

        public int AvailableReputation(IUser user) => AvailableReputation(user.Id);
        public int AvailableReputation(ulong id)
        {
            var now = DateTimeOffset.Now;
            return RegisterUser(id).Count(time => time <= now);
        }

        public async Task<ReputationResult> RepUser(Configuration config, ConfigUser user)
        {
            if(CanRep(user.ID))
            {
                user.Reputation++;
                await config.Write(DatabaseType.User);
                var nextRep = DateTimeOffset.Now.AddMilliseconds(Cooldown);
                Insert(user.ID, nextRep);
                return ReputationResult.FromSuccess(nextRep);
            }

            return ReputationResult.FromError(NextReputation(user.ID));
        }
    }

    public class ReputationResult
    {
        public DateTimeOffset NextReputation { get; }
        public bool IsSuccess { get; }

        protected ReputationResult(bool success, DateTimeOffset nextRep)
        {
            IsSuccess = success;
            NextReputation = nextRep;
        }

        public static ReputationResult FromSuccess(DateTimeOffset nextReputation)
            => new ReputationResult(true, nextReputation);

        public static ReputationResult FromError(DateTimeOffset nextReputation)
            => new ReputationResult(false, nextReputation);
    }
}
