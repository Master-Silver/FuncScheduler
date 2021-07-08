using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace FuncScheduler
{
    public class FuncWithStartTime : IFuncWithStartTime
    {
        private readonly ILogger _logger;
        private readonly DateTime _startOn;
        private readonly Action _action;
        private readonly Func<Task> _asyncFunc;
        private bool _isCompleted;

        public DateTime StartOn { get => _startOn; }
        public bool IsCompleted { get => _isCompleted; }

        /// <exception cref="ArgumentNullException"></exception>
        public FuncWithStartTime(Action action, DateTime startOn, ILogger logger = null)
        {
            _startOn = startOn;
            _action = action;
            _logger = logger;
        }

        /// <exception cref="ArgumentNullException"></exception>
        public FuncWithStartTime(Func<Task> asyncFunk, DateTime startOn, ILogger logger = null)
        {
            _startOn = startOn;
            _asyncFunc = asyncFunk;
            _logger = logger;
        }

        public int CompareTo(IFuncWithStartTime other)
        {
            return StartOn.CompareTo(other.StartOn);
        }

        public async Task RunSaveAndWithLoggerWhenSet()
        {
            try
            {
                await Run();
            }
            catch (Exception exception)
            {
                _logger?.LogCritical(exception, "Unexpected failure of a function with start time.");
            }
            finally
            {
                _isCompleted = true;
            }
        }

        private Task Run()
        {
            if (_action != null)
            {
                return Task.Run(_action);
            }

            return Task.Run(_asyncFunc);
        }
    }
}
