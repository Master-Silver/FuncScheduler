using System;
using System.Threading;
using System.Threading.Tasks;

namespace FuncScheduler.Example
{
    public class ControlStation : IDisposable
    {
        private readonly SemaphoreSlim _semaphoreForRandom = new SemaphoreSlim(1, 1);
        private readonly Random _random = new Random();
        private readonly FuncScheduler _funcScheduler = new FuncScheduler();
        private int _controlMinSec;
        private int _controlMaxSec;
        private int _nextMinSec;
        private int _nextMaxSec;

        public async Task StartControls(
            string[] controllerNames,
            int controlMinSec,
            int controlMaxSec,
            int nextMinSec,
            int nextMaxSec)
        {
            _controlMinSec = controlMinSec;
            _controlMaxSec = controlMaxSec;
            _nextMinSec = nextMinSec;
            _nextMaxSec = nextMaxSec;

            foreach (var name in controllerNames)
            {
                await SetUpControlRoutine(name);
            }
        }

        private async Task SetUpControlRoutine(string controllerName)
        {
            var startOn = DateTime.Now.AddSeconds(await NextRandom(_nextMinSec, _nextMaxSec));

            await _funcScheduler.Add(async () =>
            {
                await DoControl(controllerName);
                await SetUpControlRoutine(controllerName);
            }, startOn);

            Console.WriteLine($"{DateTime.Now:T} | {controllerName} will start his next control at {startOn:T}.");
        }

        private async Task DoControl(string controllerName)
        {
            Console.WriteLine($"{DateTime.Now:T} | {controllerName} started his control.");
            await Task.Delay(TimeSpan.FromSeconds(await NextRandom(_controlMinSec, _controlMaxSec)));
            Console.WriteLine($"{DateTime.Now:T} | {controllerName} finished his control.");
        }

        private async Task<int> NextRandom(int min, int max)
        {
            await _semaphoreForRandom.WaitAsync();

            try
            {
                return _random.Next(min, max);
            }
            finally
            {
                _semaphoreForRandom.Release();
            }
        }

        public void Dispose()
        {
            _funcScheduler?.Dispose();
            _semaphoreForRandom?.Dispose();
        }
    }
}
