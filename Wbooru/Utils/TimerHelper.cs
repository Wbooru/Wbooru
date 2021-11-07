using System;
using System.Collections.Generic;
using System.Text;

namespace Wbooru.Utils
{
    public static class TimerHelper
    {
        public class TimerInstance : IDisposable
        {
            public string Description { get; set; }
            public DateTime StartTime { get; set; }

            public void Dispose()
            {
                Stop(Description);
            }

            public void Stop(string description) => StopTimer(this, description);
        }
        
        /// <summary>
        /// Start a timer to measure time cost
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        public static TimerInstance BeginTimer(string description)
        {
            var obj = ObjectPool<TimerInstance>.Get();

            obj.Description = description;
            obj.StartTime = DateTime.Now;

            return obj;
        }

        public static void StopTimer(TimerInstance timer,string description)
        {
            var endTime = DateTime.Now;
            var spend = endTime - timer.StartTime;

            Log.Info($"Spent {spend.ToString("mm':'ss':'fff")} : {description}");
            ObjectPool<TimerInstance>.Return(timer);
        }
    }
}
