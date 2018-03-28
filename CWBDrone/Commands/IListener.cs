using Discord.Commands;
using System.Threading.Tasks;

namespace CWBDrone.Commands
{
    public interface IListener
    {
        CWBContext Context { get; set; }
        RunMode GetRunMode();
        Task ExecuteAsync();
    }
}
