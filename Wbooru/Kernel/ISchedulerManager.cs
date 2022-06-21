using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Kernel.DI;
using Wbooru.PluginExt;

namespace Wbooru.Kernel
{
    public interface ISchedulerManager : IManagerLifetime, IImplementInjectable
    {
        Task AddScheduler(ISchedulable s);
        Task RemoveScheduler(ISchedulable s);
        IEnumerable<ISchedulable> CurrentRunningSchedulers { get; }
    }
}
