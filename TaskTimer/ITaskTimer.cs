using System;
using System.Threading.Tasks;

namespace TaskTimer
{
    public interface ITaskTimer
    {
        Task<bool> TryAdd(ITaskWithTimer taskWithTime);

        /// <returns>Null when failed to add the task.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        Task<ITaskWithTimer> TryAddTask(TimeSpan startIn, Task task);

        /// <returns>Null when failed to add the task.</returns>
        Task<ITaskWithTimer> TryAddTask(DateTime startOn, Task task);
    }
}