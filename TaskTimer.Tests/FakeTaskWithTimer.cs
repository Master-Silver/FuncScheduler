using System;
using System.Threading.Tasks;

namespace TaskTimer.Tests
{
    public class FakeTaskWithTimer : ITaskWithTimer
    {
        public DateTime StartOn { get; set; }
        public Task Task { get; set; }
        public DateTime DidStartOn { get; set; }

        public FakeTaskWithTimer(DateTime startOn)
        {
            StartOn = startOn;
            Task = new Task(() =>
            {
                DidStartOn = DateTime.Now;
            });
        }

        public FakeTaskWithTimer(TimeSpan startIn) : this(DateTime.Now + startIn)
        {
        }

        public FakeTaskWithTimer(DateTime startOn, Task task)
        {
            StartOn = startOn;
            Task = task ?? throw new ArgumentNullException(nameof(task));
        }

        public FakeTaskWithTimer(TimeSpan startIn, Task task) : this(DateTime.Now + startIn, task)
        {
        }

        public int CompareTo(ITaskWithTimer other)
        {
            return StartOn.CompareTo(other.StartOn);
        }
    }
}
