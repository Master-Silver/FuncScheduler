using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TaskTimer
{
    public class TaskTimer : ITaskTimer
    {
        private readonly ILogger logger;
        private readonly SortedSet<ITaskWithTimer> tasksWithStartTime;
        private readonly SemaphoreSlim semaphore;
        private readonly System.Timers.Timer Timer;

        public TaskTimer(ILogger logger = null)
        {
            this.logger = logger;
            tasksWithStartTime = new SortedSet<ITaskWithTimer>();
            semaphore = new SemaphoreSlim(1, 1);
            Timer = new System.Timers.Timer()
            {
                AutoReset = false,
            };
            Timer.Elapsed += OnTimedEvent;
        }

        public async Task<ITaskWithTimer> TryAddTask(TimeSpan startIn, Task task)
        {
            ITaskWithTimer taskWithTimer = new TaskWithTimer(startIn, task);

            if (await TryAdd(taskWithTimer))
            {
                return taskWithTimer;
            }

            return null;
        }

        public async Task<ITaskWithTimer> TryAddTask(DateTime startOn, Task task)
        {
            ITaskWithTimer taskWithTimer = new TaskWithTimer(startOn, task);

            if (await TryAdd(taskWithTimer))
            {
                return taskWithTimer;
            }

            return null;
        }

        public async Task<bool> TryAdd(ITaskWithTimer taskWithTimer)
        {
            await semaphore.WaitAsync();

            try
            {
                if ((taskWithTimer.StartOn - DateTime.Now).TotalMilliseconds > int.MaxValue)
                {
                    return false;
                }

                if (taskWithTimer.Task.Status != TaskStatus.Created)
                {
                    return false;
                }

                if (tasksWithStartTime.Add(taskWithTimer))
                {
                    StartTimerWithNewInterval();

                    return true;
                }

                return false;
            }
            finally
            {
                semaphore.Release();
            }
        }

        private void StartTimerWithNewInterval()
        {
            if (Timer.Enabled)
            {
                Timer.Stop();
            }

            if (tasksWithStartTime.Min != null)
            {
                double nextInterval = (tasksWithStartTime.Min.StartOn - DateTime.Now).TotalMilliseconds;

                if (nextInterval < 1 || nextInterval >= int.MaxValue)
                {
                    nextInterval = 1;
                }

                Timer.Interval = nextInterval;
                Timer.Start();
            }
        }

        private async void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            IList<Task> listOfTasks = await RunTasks();

            if (logger != null)
            {
                while (listOfTasks.Count >= 1)
                {
                    Task task = await Task.WhenAny(listOfTasks);

                    if (null != task.Exception)
                    {
                        logger.LogCritical(task.Exception, "Unexpected failure of a task with timer.");
                    }

                    listOfTasks.Remove(task);
                }
            }
        }

        private async Task<IList<Task>> RunTasks()
        {
            await semaphore.WaitAsync();

            IList<Task> listOfTasks = new List<Task>();

            try
            {
                while (tasksWithStartTime.Min != null && tasksWithStartTime.Min.StartOn <= DateTime.Now)
                {
                    ITaskWithTimer toRun = tasksWithStartTime.Min;
                    tasksWithStartTime.Remove(toRun);

                    if (toRun.Task.Status == TaskStatus.Created)
                    {
                        toRun.Task.Start();
                        listOfTasks.Add(toRun.Task);
                    }
                }

                StartTimerWithNewInterval();

                return listOfTasks;
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
