﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Wbooru.Kernel.DI;

namespace Wbooru.PluginExt
{
    public interface ISchedulable : IMultiImplementProvidable
    {
        string SchedulerName { get; }

        TimeSpan ScheduleCallLoopInterval { get; }

        Task OnScheduleCall(CancellationToken cancellationToken);

        void OnSchedulerTerm();
    }
}