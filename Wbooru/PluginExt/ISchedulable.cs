namespace Wbooru.PluginExt
{
    public interface ISchedulable
    {
        string SchedulerName { get; }

        bool IsAsyncSchedule { get; }

        void OnScheduleCall();

        void OnSchedulerTerm();
    }
}