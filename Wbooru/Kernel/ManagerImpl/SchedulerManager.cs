using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wbooru.Kernel.DI;
using Wbooru.PluginExt;
using Wbooru.Utils;

namespace Wbooru.Kernel.ManagerImpl
{
    [PriorityExport(typeof(ISchedulerManager), Priority = 0)]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class SchedulerManager : ISchedulerManager
    {
        private AbortableThread runThread;

        private List<ISchedulable> schedulers { get; } = new List<ISchedulable>();

        private Dictionary<ISchedulable, DateTime> schedulersCallTime { get; } = new();

        public IEnumerable<ISchedulable> Schedulers => schedulers;

        public Task OnInit()
        {
            foreach (var s in Container.GetAll<ISchedulable>())
                AddScheduler(s);

            runThread = new AbortableThread(Run);
            runThread.Name = "SchedulerManager::Run()";
            runThread.Start();

            return Task.CompletedTask;
        }

        public Task AddScheduler(ISchedulable s)
        {
            if (s is null || schedulers.FirstOrDefault(x => x.SchedulerName.Equals(s.SchedulerName)) != null)
            {
                Log.Warn($"Can't add scheduler : {s?.SchedulerName} is null/exist.");
                return Task.CompletedTask;
            }

            schedulers.Add(s);
            schedulersCallTime[s] = DateTime.MinValue;
            Log.Info("Added new scheduler: " + s.SchedulerName);

            return Task.CompletedTask;
        }

        private async void Run(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var schedulers = Schedulers
                        .Where(x => DateTime.Now - schedulersCallTime[x] >= x.ScheduleCallLoopInterval)
                        .Select(x => x.OnScheduleCall(cancellationToken).ContinueWith(_ => schedulersCallTime[x] = DateTime.Now))
                        .ToArray();
                    if (schedulers.Length == 0)
                        await Task.Delay(500, cancellationToken);
                    await Task.WhenAll(schedulers);
                    await Task.Yield();
                }
                catch (TaskCanceledException)
                {

                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public Task OnExit()
        {
            try
            {
                runThread.Abort();
            }
            catch { }

            foreach (var scheduler in Schedulers)
            {
                Log.Info("Call OnSchedulerTerm() :" + scheduler.SchedulerName);
                scheduler.OnSchedulerTerm();
            }

            return Task.CompletedTask;
        }

        public Task RemoveScheduler(ISchedulable s)
        {
            if (s is null || schedulers.FirstOrDefault(x => x.SchedulerName.Equals(s.SchedulerName)) is null)
            {
                Log.Warn($"Can't remove scheduler : {s?.SchedulerName} is null or not exist.");
                return Task.CompletedTask;
            }

            schedulers.Remove(s);
            Log.Info("Remove scheduler: " + s.SchedulerName);

            return Task.CompletedTask;
        }
    }
}
