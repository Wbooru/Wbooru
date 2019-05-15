namespace Wbooru.PluginExt
{
    public interface ISchedulable
    {
        bool IsAsyncSchedule { get; } 

        void OnScheduleCall();
    }
}