using System;
using System.Threading;
using System.Threading.Tasks;

namespace Wbooru.PluginExt
{
    public interface ISchedulable
    {
        string SchedulerName { get; }

        TimeSpan ScheduleCallLoopInterval { get; }

        Task OnScheduleCall(CancellationToken cancellationToken);

        void OnSchedulerTerm();
    }
}