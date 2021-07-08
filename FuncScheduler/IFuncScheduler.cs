using System;
using System.Threading.Tasks;

namespace FuncScheduler
{
    public interface IFuncScheduler
    {
        bool TimerIsActive { get; }
        int Count { get; }

        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="AddException"></exception>
        Task<IFuncWithStartTime> Add(Action action, TimeSpan startIn);

        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="AddException"></exception>
        Task<IFuncWithStartTime> Add(Func<Task> asyncFunc, TimeSpan startIn);

        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="AddException"></exception>
        Task<IFuncWithStartTime> Add(Action action, DateTime startOn);

        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="AddException"></exception>
        Task<IFuncWithStartTime> Add(Func<Task> asyncFunc, DateTime startOn);

        Task<bool> TryRemove(IFuncWithStartTime funcWithStartTime);

        Task RunAllAddedTasksNowAndClear();
    }
}