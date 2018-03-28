using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWBDrone
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            var drone = new CWBDrone(@"D:\vs-workspace\BoltEnergized\config.json");
            await drone.LoadAsync();
            await drone.StartAsync();
            await drone.StartConsole();
        }
    }
}
