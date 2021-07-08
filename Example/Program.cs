using System;

namespace FuncScheduler.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            using var controlStation = new ControlStation();

            Console.WriteLine("How long should this app run in seconds:");
            var runtime = TimeSpan.FromSeconds(int.Parse(Console.ReadLine()));

            Console.WriteLine("Minimum seconds needed for a control:");
            int controlMinSec = int.Parse(Console.ReadLine());

            Console.WriteLine("Maximum seconds needed for a control:");
            int controlMaxSec = int.Parse(Console.ReadLine());

            Console.WriteLine("Minimum seconds to wait before starting the next control:");
            int nextMinSec = int.Parse(Console.ReadLine());

            Console.WriteLine("Maximum seconds to wait before starting the next control:");
            int nextMaxSec = int.Parse(Console.ReadLine());

            Console.WriteLine("Names of the controllers separated with space:");
            string[] controllerNames = Console.ReadLine().Split(' ');

            controlStation.StartControls(
                controllerNames,
                controlMinSec,
                controlMaxSec,
                nextMinSec,
                nextMaxSec).GetAwaiter().GetResult();

            System.Threading.Thread.Sleep(runtime);
        }
    }
}
