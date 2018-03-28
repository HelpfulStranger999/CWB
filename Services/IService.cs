using System.Threading.Tasks;

namespace CWBDrone.Services
{
    public interface IService
    {
        Task<bool> ReadyToDisconnect(CWBDrone bot);
        Task Disconnect();
    }
}
