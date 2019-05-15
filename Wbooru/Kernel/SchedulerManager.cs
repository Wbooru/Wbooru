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
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(SchedulerManager))]
    public class SchedulerManager
    {
        [ImportMany(typeof(ISchedulable))]
        public IEnumerable<ISchedulable> Schedulers { get; set; }

        Thread schedule_thread;

        public SchedulerManager()
        {
            Init();
        }

        private void Init()
        {
            schedule_thread = new Thread(Run);
            schedule_thread.Name = "SchedulerManager Thread";
            schedule_thread.IsBackground = true;
            schedule_thread.Start();
        }

        private void Run()
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

        ~SchedulerManager()
        {
            schedule_thread.Abort();
        }
    }
}
