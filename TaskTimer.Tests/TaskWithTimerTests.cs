using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace TaskTimer.Tests
{
    public class TaskWithTimerTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(100)]
        public void Should_ReturnMaxAndMin_When_TimesAreRandom(int numberOfEntrys)
        {
            Random random = new Random();
            SortedSet<ITaskWithTimer> sortedTaskTimer = new SortedSet<ITaskWithTimer>();
            ITaskWithTimer maxValue = null;
            ITaskWithTimer minValue = null;

            for (int i = 0; i < numberOfEntrys; i++)
            {
                ITaskWithTimer taskWithTime = new TaskWithTimer(
                    new TimeSpan(0, 0, 0, 0, random.Next(0, numberOfEntrys - 1)),
                    new Task(() => { })
                );

                if (i < 1)
                {
                    maxValue = taskWithTime;
                    minValue = taskWithTime;
                }
                else
                {
                    if (taskWithTime.StartOn > maxValue.StartOn)
                    {
                        maxValue = taskWithTime;
                    }

                    if (taskWithTime.StartOn < minValue.StartOn)
                    {
                        minValue = taskWithTime;
                    }
                }

                Assert.True(sortedTaskTimer.Add(taskWithTime));
            }

            Assert.Equal(maxValue.StartOn, sortedTaskTimer.Max.StartOn);
            Assert.Equal(minValue.StartOn, sortedTaskTimer.Min.StartOn);
        }

        [Fact]
        public void Should_ReturnRightSortOrder_When_CompareToIsUsed()
        {
            ITaskWithTimer taskWithBiggerTime = new TaskWithTimer(
                new TimeSpan(0, 0, 0, 0, 2),
                new Task(() => { })
            );
            ITaskWithTimer taskWithSmallerTime = new TaskWithTimer(
                new TimeSpan(0, 0, 0, 0, 1),
                new Task(() => { })
            );

            Assert.Equal(-1, taskWithSmallerTime.CompareTo(taskWithBiggerTime));
            Assert.Equal(0, taskWithSmallerTime.CompareTo(taskWithSmallerTime));
            Assert.Equal(0, taskWithBiggerTime.CompareTo(taskWithBiggerTime));
            Assert.Equal(1, taskWithBiggerTime.CompareTo(taskWithSmallerTime));
        }

        [Fact]
        public void Should_ThrowArgumentNullException_When_GivenTaskIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                ITaskWithTimer taskWithTime = new TaskWithTimer(
                    DateTime.Now,
                    null
                );
            });
        }

        [Fact]
        public void Should_ThrowArgumentOutOfRangeException_When_GivenTimeSpanIsToLong()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                ITaskWithTimer taskWithTime = new TaskWithTimer(
                    TimeSpan.MaxValue,
                    new Task(() => { })
                );
            });
        }

        [Fact]
        public void Should_ThrowNullReferenceException_When_ComparedToNull()
        {
            ITaskWithTimer taskWithTime = new TaskWithTimer(
                new TimeSpan(0, 0, 0, 0, 1),
                new Task(() => { })
            );

            Assert.Throws<NullReferenceException>(() =>
            {
                taskWithTime.CompareTo(null);
            });
        }
    }
}
