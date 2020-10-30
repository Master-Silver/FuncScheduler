using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace TaskTimer.Tests
{
    public class TaskTimerTests
    {
        [Fact]
        public async Task Should_RunSuccessful_When_AddedITaskWithStartTime()
        {
            int runIn = 5;
            Stopwatch stopwatch = new Stopwatch();
            ITaskTimer taskTimer = new TaskTimer();
            FakeTaskWithTimer fakeTaskTime = new FakeTaskWithTimer(new TimeSpan(0, 0, 0, 0, runIn));

            stopwatch.Start();
            Assert.True(await taskTimer.TryAdd(fakeTaskTime));
            await fakeTaskTime.Task;
            stopwatch.Stop();
            Assert.InRange(stopwatch.ElapsedMilliseconds, runIn, runIn + 15);
        }

        [Fact]
        public async Task Should_RunSuccessful_When_AddedTaskWithTimeSpan()
        {
            int runIn = 5;
            Stopwatch stopwatch = new Stopwatch();
            ITaskTimer taskTimer = new TaskTimer();
            Task task = new Task(() => { });

            stopwatch.Start();
            ITaskWithTimer taskWithTimer = await taskTimer.TryAddTask(new TimeSpan(0, 0, 0, 0, runIn), task);
            Assert.NotNull(taskWithTimer);
            await taskWithTimer.Task;
            stopwatch.Stop();
            Assert.InRange(stopwatch.ElapsedMilliseconds, runIn, runIn + 15);
        }

        [Fact]
        public async Task Should_RunSuccessful_When_AddedTaskWithDateTime()
        {
            int runIn = 5;
            Stopwatch stopwatch = new Stopwatch();
            ITaskTimer taskTimer = new TaskTimer();
            Task task = new Task(() => { });

            stopwatch.Start();
            ITaskWithTimer taskWithTimer = await taskTimer.TryAddTask(DateTime.Now.AddMilliseconds(5), task);
            Assert.NotNull(taskWithTimer);
            await taskWithTimer.Task;
            stopwatch.Stop();
            Assert.InRange(stopwatch.ElapsedMilliseconds, runIn, runIn + 15);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(2, 0)]
        [InlineData(2, 10)]
        [InlineData(2, 100)]
        [InlineData(2, 1000)]
        [InlineData(5, 0)]
        [InlineData(5, 10)]
        [InlineData(5, 100)]
        [InlineData(5, 500)]
        [InlineData(10, 0)]
        [InlineData(10, 10)]
        [InlineData(10, 100)]
        [InlineData(100, 0)]
        [InlineData(100, 10)]
        public async Task Should_RunTasksInTime_When_TasksAdded(int numberOfTasks, int timeBetweenTask)
        {
            ITaskTimer taskTimer = new TaskTimer();
            IList<FakeTaskWithTimer> listOfTasksWithTime = new List<FakeTaskWithTimer>(numberOfTasks);
            IList<Task> listOfTasks = new List<Task>(numberOfTasks);

            for (int i = 0; i < numberOfTasks; i++)
            {
                FakeTaskWithTimer fakeTaskTime = new FakeTaskWithTimer(new TimeSpan(0, 0, 0, 0, timeBetweenTask * i));
                listOfTasksWithTime.Add(fakeTaskTime);
                listOfTasks.Add(fakeTaskTime.Task);
                Assert.True(await taskTimer.TryAdd(fakeTaskTime));
            }

            while (listOfTasks.Count >= 1)
            {
                Task completedTask = await Task.WhenAny(listOfTasks);

                foreach (var fakeTaskTime in listOfTasksWithTime)
                {
                    if (fakeTaskTime.Task == completedTask)
                    {
                        Assert.InRange((fakeTaskTime.DidStartOn - fakeTaskTime.StartOn).TotalMilliseconds, 0, 30);
                        listOfTasksWithTime.Remove(fakeTaskTime);
                        break;
                    }
                }

                listOfTasks.Remove(completedTask);
            }
        }

        [Fact]
        public async Task Should_ReturnFalse_When_RunOnIsToFarInTheFuture()
        {
            ITaskTimer taskTimer = new TaskTimer();
            FakeTaskWithTimer fakeTaskTime = new FakeTaskWithTimer(DateTime.MaxValue);

            Assert.False(await taskTimer.TryAdd(fakeTaskTime));
        }

        [Fact]
        public async Task Should_ReturnNull_When_RunOnIsToFarInTheFuture()
        {
            ITaskTimer taskTimer = new TaskTimer();
            Task task = new Task(() => { });

            Assert.Null(await taskTimer.TryAddTask(DateTime.MaxValue, task));
        }

        [Fact]
        public async Task Should_ThrowArgumentOutOfRangeException_When_TimeSpanIsToLong()
        {
            ITaskTimer taskTimer = new TaskTimer();
            Task task = new Task(() => { });

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await taskTimer.TryAddTask(TimeSpan.MaxValue, task);
            });
        }

        [Fact]
        public async Task Should_ReturnNull_When_TaskStatusIsNotCreated()
        {
            ITaskTimer taskTimer = new TaskTimer();
            Task task = new Task(() => { });
            task.Start();

            Assert.Null(await taskTimer.TryAddTask(DateTime.Now.AddMilliseconds(5), task));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(100)]
        public async Task Should_LogExceptions_When_TasksThrowThem(int numberOfTasks)
        {
            FakeLogger fakeLogger = new FakeLogger();
            ITaskTimer taskTimer = new TaskTimer(fakeLogger);

            for (int i = 0; i < numberOfTasks; i++)
            {
                Task task = new Task(() =>
                {
                    throw new Exception("Eine Test");
                });

                Assert.NotNull(await taskTimer.TryAddTask(new TimeSpan(0, 0, 0, 0, i), task));
            }

            for (int i = 0; fakeLogger.Logs.Count < numberOfTasks; i++)
            {
                await Task.Delay(numberOfTasks);

                if (i > 3)
                {
                    break;
                }
            }

            Assert.Equal(numberOfTasks, fakeLogger.Logs.Count);

            for (int i = 0; i < fakeLogger.Logs.Count; i++)
            {
                Assert.IsType<AggregateException>(fakeLogger.Logs[i].Exception);
            }
        }
    }
}
