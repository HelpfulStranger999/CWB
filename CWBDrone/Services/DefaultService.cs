using System.Threading.Tasks;

namespace CWBDrone.Services
{
    public abstract class DefaultService : IService
    {
        protected bool Disconnecting = false;
        public virtual Task Disconnect()
            => Task.CompletedTask;

        public virtual Task<bool> ReadyToDisconnect(CWBDrone bot)
        {
            Disconnecting = true;
            return Task.FromResult(true);
        }
    }
}
