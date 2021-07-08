using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FuncScheduler
{
    public class FuncScheduler : IFuncScheduler, IDisposable
    {
        private readonly ILogger _logger;
        private readonly SortedSet<IFuncWithStartTime> _sortedTasksWithTimer;
        private readonly SemaphoreSlim _semaphore;
        private readonly System.Timers.Timer _timer;

        public bool TimerIsActive { get => _timer.Enabled; }
        public int Count { get => _sortedTasksWithTimer.Count; }

        public FuncScheduler(ILogger logger = null)
        {
            _logger = logger;
            _sortedTasksWithTimer = new SortedSet<IFuncWithStartTime>();
            _semaphore = new SemaphoreSlim(1, 1);
            _timer = new System.Timers.Timer()
            {
                AutoReset = false,
            };
            _timer.Elapsed += OnTimedEvent;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _semaphore?.Dispose();
        }

        public Task<IFuncWithStartTime> Add(Action action, TimeSpan startIn)
        {
            return Add(action, DateTime.Now + startIn);
        }

        public Task<IFuncWithStartTime> Add(Func<Task> asyncFunc, TimeSpan startIn)
        {
            return Add(asyncFunc, DateTime.Now + startIn);
        }

        public async Task<IFuncWithStartTime> Add(Action action, DateTime startOn)
        {
            var funcWithStartTime = new FuncWithStartTime(action, startOn, _logger);

            await Add(funcWithStartTime);

            return funcWithStartTime;
        }

        public async Task<IFuncWithStartTime> Add(Func<Task> asyncFunc, DateTime startOn)
        {
            var funcWithStartTime = new FuncWithStartTime(asyncFunc, startOn, _logger);

            await Add(funcWithStartTime);

            return funcWithStartTime;
        }

        private async Task Add(FuncWithStartTime funcWithStartTime)
        {
            await _semaphore.WaitAsync();

            try
            {
                CheckStartTime(funcWithStartTime.StartOn);

                if (false == _sortedTasksWithTimer.Add(funcWithStartTime))
                {
                    throw new AddException($"Adding to the {nameof(SortedSet<object>)} {nameof(_sortedTasksWithTimer)} failed unexpected.");
                }

                StartTimerWithNewInterval();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<bool> TryRemove(IFuncWithStartTime funcWithStartTime)
        {
            await _semaphore.WaitAsync();

            try
            {
                if (_sortedTasksWithTimer.Remove(funcWithStartTime))
                {
                    StartTimerWithNewInterval();

                    return true;
                }

                return false;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task RunAllAddedTasksNowAndClear()
        {
            List<Task> taskList = new List<Task>();

            await _semaphore.WaitAsync();

            try
            {
                if (_timer.Enabled)
                {
                    _timer.Stop();
                }

                foreach (var toRun in _sortedTasksWithTimer)
                {
                    taskList.Add(toRun.RunSaveAndWithLoggerWhenSet());
                }

                _sortedTasksWithTimer.Clear();
            }
            finally
            {
                _semaphore.Release();
            }

            await Task.WhenAll(taskList);
        }

        private void CheckStartTime(DateTime startOn)
        {
            if ((startOn - DateTime.Now).TotalMilliseconds > int.MaxValue)
            {
                throw new ArgumentException(
                    "The Milliseconds to elapse to the start point are bigger than max int.",
                    nameof(startOn));
            }
        }

        private void StartTimerWithNewInterval()
        {
            if (_timer.Enabled)
            {
                _timer.Stop();
            }

            if (_sortedTasksWithTimer.Min != null)
            {
                double nextInterval = (_sortedTasksWithTimer.Min.StartOn - DateTime.Now).TotalMilliseconds;

                if (nextInterval < 1 || nextInterval >= int.MaxValue)
                {
                    nextInterval = 1;
                }

                _timer.Interval = nextInterval;
                _timer.Start();
            }
        }

        private async void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            await RunElapsedFuncsWithStartTime();
        }

        private async Task RunElapsedFuncsWithStartTime()
        {
            List<Task> taskList = new List<Task>();

            await _semaphore.WaitAsync();

            try
            {
                while (_sortedTasksWithTimer.Min != null
                    && _sortedTasksWithTimer.Min.StartOn <= DateTime.Now)
                {
                    var toRun = _sortedTasksWithTimer.Min;

                    _sortedTasksWithTimer.Remove(toRun);
                    taskList.Add(toRun.RunSaveAndWithLoggerWhenSet());
                }

                StartTimerWithNewInterval();
            }
            finally
            {
                _semaphore.Release();
            }

            await Task.WhenAll(taskList);
        }
    }
}
