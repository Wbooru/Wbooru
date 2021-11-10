using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wbooru.PluginExt;
using Wbooru.Utils;

namespace Wbooru.Kernel
{
    public static class SchedulerManager
    {
        private static AbortableThread runThread;

        private static List<ISchedulable> schedulers { get; } = new List<ISchedulable>();

        private static Dictionary<ISchedulable, DateTime> schedulersCallTime { get; } = new();

        public static IEnumerable<ISchedulable> Schedulers => schedulers;

        public static void Init()
        {
            foreach (var s in Container.Default.GetExportedValues<ISchedulable>())
                AddScheduler(s);

            runThread = new AbortableThread(Run);
            runThread.Start();
        }

        public static void AddScheduler(ISchedulable s)
        {
            if (s is null || schedulers.FirstOrDefault(x => x.SchedulerName.Equals(s.SchedulerName)) != null)
            {
                Log.Warn($"Can't add scheduler : {s?.SchedulerName} is null/exist.");
                return;
            }

            schedulers.Add(s);
            schedulersCallTime[s] = DateTime.MinValue;
            Log.Info("Added new scheduler: " + s.SchedulerName);
        }

        private static async void Run(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var schedulers = Schedulers
                    .Where(x => DateTime.Now - schedulersCallTime[x] >= x.ScheduleCallLoopInterval)
                    .Select(x => x.OnScheduleCall(cancellationToken).ContinueWith(_ => schedulersCallTime[x] = DateTime.Now))
                    .ToArray();
                await Task.WhenAll(schedulers);
                await Task.Yield();
            }
        }

        public static void Term()
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
        }
    }
}
