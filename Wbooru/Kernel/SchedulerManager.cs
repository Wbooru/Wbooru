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

        private static Dictionary<ISchedulable, bool> schedulersAsyncLocker { get; } = new();
        private static Dictionary<ISchedulable, DateTime> schedulersCallTime { get; } = new();

        public static IEnumerable<ISchedulable> Schedulers => schedulers;

        private static CancellationTokenSource schedule_thread_cancel_token = new CancellationTokenSource();

        public static void Init()
        {
            schedule_thread_cancel_token = new CancellationTokenSource();
            Task.Run(Run, schedule_thread_cancel_token.Token);

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
            schedulersAsyncLocker[s] = false;
            schedulersCallTime[s] = DateTime.MinValue;
            Log.Info("Added new scheduler: " + s.SchedulerName);
        }

        private static async void Run()
        {
            while (!schedule_thread_cancel_token.IsCancellationRequested)
            {
                for (int i = 0; i < Schedulers.Count(); i++)
                {
                    var schedule = Schedulers.ElementAt(i);

                    if (DateTime.Now - schedulersCallTime[schedule] < schedule.ScheduleCallLoopInterval)
                        continue;

                    if (schedule.IsAsyncSchedule)
                    {
                        if (schedulersAsyncLocker[schedule])
                            continue;
                        schedulersAsyncLocker[schedule] = true;

#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                        Task.Run(async () =>
                        {
                            await schedule.OnScheduleCall(schedule_thread_cancel_token.Token);
                            schedulersAsyncLocker[schedule] = false;
                            schedulersCallTime[schedule] = DateTime.Now;
                        });
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                    }
                    else
                    {
                        await schedule.OnScheduleCall(schedule_thread_cancel_token.Token);
                        schedulersCallTime[schedule] = DateTime.Now;
                    }
                }

                await Task.Yield();
            }
        }

        public static void Term()
        {
            try
            {
                schedule_thread_cancel_token.Cancel();
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
