using System;
using System.Threading.Tasks;

namespace TaskTimer
{
    public class TaskWithTimer : ITaskWithTimer
    {
        //public static ITaskTimer DefaultTaskTimer { get; } = new TaskTimer();

        public DateTime StartOn { get; }
        public Task Task { get; }
        //public ITaskTimer Tasktimer { get; private set; }

        /// <exception cref="ArgumentNullException"></exception>
        public TaskWithTimer(DateTime startOn, Task task)
        {
            StartOn = startOn;
            Task = task ?? throw new ArgumentNullException(nameof(task));
        }

        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public TaskWithTimer(TimeSpan startIn, Task task) : this(DateTime.Now + startIn, task)
        {
        }

        /*public async Task<bool> TryAddToDefaultTimer()
        {
            throw new NotImplementedException();
        }*/

        public int CompareTo(ITaskWithTimer other)
        {
            return StartOn.CompareTo(other.StartOn);
        }
    }
}