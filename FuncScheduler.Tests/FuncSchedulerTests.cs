using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace FuncScheduler.Tests
{
    public class FuncSchedulerTests
    {
        private readonly TimeSpan _errorMargen = TimeSpan.FromMilliseconds(30);

        [Fact]
        public async Task Should_RunSuccessful_When_AddedActionWithStartTime()
        {
            TimeSpan runIn = TimeSpan.FromMilliseconds(5);
            Stopwatch stopwatch = new Stopwatch();
            using var funcScheduler = new FuncScheduler();
            IFuncWithStartTime funcWithStartTime =
                await funcScheduler.Add(() =>
                {
                    stopwatch.Stop();
                }, runIn);

            stopwatch.Start();
            Assert.True(funcScheduler.TimerIsActive);

            while (funcWithStartTime.IsCompleted == false)
            {
                await Task.Delay(runIn);
            }

            Assert.InRange(stopwatch.Elapsed, runIn, runIn + _errorMargen);
            Assert.False(funcScheduler.TimerIsActive);
        }

        [Fact]
        public async Task Should_RunSuccessful_When_AddedAsyncFuncWithStartTime()
        {
            TimeSpan runIn = TimeSpan.FromMilliseconds(5);
            Stopwatch stopwatch = new Stopwatch();
            using var funcScheduler = new FuncScheduler();
            IFuncWithStartTime funcWithStartTime =
                await funcScheduler.Add(async () =>
                {
                    await Task.Yield();
                    stopwatch.Stop();
                }, runIn);

            stopwatch.Start();
            Assert.True(funcScheduler.TimerIsActive);

            while (funcWithStartTime.IsCompleted == false)
            {
                await Task.Delay(runIn);
            }

            Assert.InRange(stopwatch.Elapsed, runIn, runIn + _errorMargen);
            Assert.False(funcScheduler.TimerIsActive);
        }

        [Fact]
        public async Task Should_NotRun_When_FuncWithTimerIsRemoved()
        {
            using var funcScheduler = new FuncScheduler();
            IFuncWithStartTime funcWithStartTime =
                await funcScheduler.Add(() => { }, TimeSpan.FromMilliseconds(1));

            Assert.True(await funcScheduler.TryRemove(funcWithStartTime));
            Assert.False(funcScheduler.TimerIsActive);
            await Task.Delay(_errorMargen);
            Assert.False(funcWithStartTime.IsCompleted);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(2, 0)]
        [InlineData(2, 10)]
        [InlineData(2, 100)]
        [InlineData(7, 0)]
        [InlineData(7, 100)]
        [InlineData(100, 0)]
        [InlineData(100, 1)]
        public async Task Should_RunAllActionsInTime_When_Added(int actionCount, int msBetweenActions)
        {
            using var funcScheduler = new FuncScheduler();
            var funcWithStartTimeList = new List<IFuncWithStartTime>(actionCount);

            for (int i = 0; i < actionCount; i++)
            {
                funcWithStartTimeList.Add(await funcScheduler.Add(
                    () => { }, TimeSpan.FromMilliseconds(msBetweenActions * i)));
            }

            Assert.True(funcScheduler.TimerIsActive);

            do
            {
                await Task.Delay(_errorMargen + TimeSpan.FromMilliseconds(msBetweenActions * actionCount));

                for (int i = funcWithStartTimeList.Count - 1; i >= 0; i--)
                {
                    if (funcWithStartTimeList[i].IsCompleted)
                    {
                        funcWithStartTimeList.RemoveAt(i);
                    }
                }
            } while (funcWithStartTimeList.Count >= 1);

            Assert.False(funcScheduler.TimerIsActive);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(2, 0)]
        [InlineData(2, 10)]
        [InlineData(2, 100)]
        [InlineData(7, 0)]
        [InlineData(7, 100)]
        [InlineData(100, 0)]
        [InlineData(100, 1)]
        public async Task Should_RunAllAddedActionsNow_When_Called(int actionCount, int msBetweenActions)
        {
            using var funcScheduler = new FuncScheduler();
            var funcWithStartTimeList = new List<IFuncWithStartTime>(actionCount);

            for (int i = 0; i < actionCount; i++)
            {
                funcWithStartTimeList.Add(await funcScheduler.Add(
                    () => { }, TimeSpan.FromMilliseconds(msBetweenActions * i)));
            }

            Assert.True(funcScheduler.TimerIsActive);
            await funcScheduler.RunAllAddedTasksNowAndClear();
            Assert.False(funcScheduler.TimerIsActive);

            foreach (var funcWithStartTime in funcWithStartTimeList)
            {
                Assert.True(funcWithStartTime.IsCompleted);
            }
        }

        [Fact]
        public async Task Should_ThrowArgumentException_When_StartOnIsToFarInTheFuture()
        {
            using var funcScheduler = new FuncScheduler();

            await Assert.ThrowsAsync<ArgumentException>("startOn", async () =>
            {
                await funcScheduler.Add(() => { }, DateTime.MaxValue);
            });
            Assert.False(funcScheduler.TimerIsActive);
        }

        [Fact]
        public async Task Should_ThrowArgumentOutOfRangeException_When_StartInIsToLong()
        {
            using var funcScheduler = new FuncScheduler();

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>("t", async () =>
            {
                await funcScheduler.Add(() => { }, TimeSpan.MaxValue);
            });
            Assert.False(funcScheduler.TimerIsActive);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(7)]
        [InlineData(100)]
        public async Task Should_LogExceptions_When_TasksThrowThem(int funcCount)
        {
            FakeLogger fakeLogger = new FakeLogger();
            using var funcScheduler = new FuncScheduler(fakeLogger);
            var funcWithStartTimeList = new List<IFuncWithStartTime>(funcCount);

            for (int i = 0; i < funcCount; i++)
            {
                funcWithStartTimeList.Add(
                    await funcScheduler.Add(async () =>
                    {
                        await Task.Yield();
                        throw new Exception("Ein Test");
                    },
                    TimeSpan.FromMilliseconds(i)));
            }

            do
            {
                await Task.Delay(_errorMargen + TimeSpan.FromMilliseconds(funcCount));

                for (int i = funcWithStartTimeList.Count - 1; i >= 0; i--)
                {
                    if (funcWithStartTimeList[i].IsCompleted)
                    {
                        funcWithStartTimeList.RemoveAt(i);
                    }
                }
            } while (funcWithStartTimeList.Count >= 1);

            Assert.False(funcScheduler.TimerIsActive);
            Assert.Equal(funcCount, fakeLogger.Logs.Count);

            foreach (var log in fakeLogger.Logs)
            {
                Assert.IsType<Exception>(log.Exception);
            }
        }
    }
}
