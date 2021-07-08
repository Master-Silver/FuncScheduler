using System;
using System.Threading.Tasks;

namespace FuncScheduler
{
    public interface IFuncWithStartTime : IComparable<IFuncWithStartTime>
    {
        DateTime StartOn { get; }
        bool IsCompleted { get; }

        Task RunSaveAndWithLoggerWhenSet();
    }
}