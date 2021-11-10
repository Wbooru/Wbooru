using System;
using System.Threading;
using System.Threading.Tasks;

namespace Wbooru.PluginExt
{
    public interface ISchedulable
    {
        string SchedulerName { get; }

        TimeSpan ScheduleCallLoopInterval { get; }

        bool IsAsyncSchedule { get; }

        Task OnScheduleCall(CancellationToken cancellationToken);

        void OnSchedulerTerm();
    }
}