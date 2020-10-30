using System;
using System.Threading.Tasks;

namespace TaskTimer
{
    public interface ITaskWithTimer : IComparable<ITaskWithTimer>
    {
        DateTime StartOn { get; }
        Task Task { get; }
    }
}