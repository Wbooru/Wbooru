using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wbooru.PluginExt;

namespace Wbooru.Kernel
{
    public static class SchedulerManager
    {
        private static List<ISchedulable> schedulers { get; } = new List<ISchedulable>();

        public static IEnumerable<ISchedulable> Schedulers => schedulers;

        private static Thread schedule_thread;
        
        public static void Init()
        {
            schedule_thread = new Thread(Run);
            schedule_thread.Name = "SchedulerManager Thread";
            schedule_thread.SetApartmentState(ApartmentState.STA);
            schedule_thread.IsBackground = true;
            schedule_thread.Start();

            foreach (var s in Container.Default.GetExportedValues<ISchedulable>())
            {
                AddScheduler(s);
            }
        }

        public static void AddScheduler(ISchedulable s)
        {
            if (s is null || schedulers.Contains(s))
                return;

            schedulers.Add(s);
            Log.Info("Added new scheduler: "+s.SchedulerName);
        }

        private static void Run()
        {
            while (true)
            {
                foreach (var schedule in Schedulers)
                {
                    if (schedule.IsAsyncSchedule)
                        Task.Run(schedule.OnScheduleCall);
                    else
                        schedule.OnScheduleCall();
                }

                Thread.Sleep(10);
            }
        }

        public static void Term()
        {
            try
            {
                schedule_thread.Abort();
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
