using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Kernel;
using Wbooru.Network;
using Wbooru.PluginExt;
using static Wbooru.Utils.Debugs.ConsoleWindow.Command;

namespace Wbooru.Utils.Debugs
{
    internal static class ConsoleCommandProcessor
    {
        private static bool autoClearConsole = false;

        internal static void InitDefault()
        {
            ConsoleWindow.OnProcessConsoleInputCommandEvent += ConsoleWindow_OnProcessConsoleInputCommandEvent;
        }

        private static bool ConsoleWindow_OnProcessConsoleInputCommandEvent(ConsoleWindow.Command command)
        {
            if (autoClearConsole)
                Console.Clear();

            switch (command.Name.ToLower())
            {
                case "clear":
                    Console.Clear();
                    break;
                case "console":
                    OnConsole(command);
                    break;
                case "schedulers":
                    OnSchedulers(command);
                    break;
                case "images":
                    OnImages(command);
                    break;
                default:
                    return false;
            }

            return true;
        }

        private static void OnImages(ConsoleWindow.Command command)
        {
            var fetcher = Container.Get<ImageFetchDownloadScheduler>();
            if (command.Args.Any(x => x.Name.ToLower() == "status"))
            {
                Log.Debug($"----Image Downloader Waiting Tasks ({fetcher.TasksWaitingQueue.Count()})----");
                foreach ((var taskInfo, var i) in fetcher.TasksWaitingQueue.Select((x, i) => (x, i)))
                {
                    Log.Debug($"{i} : Task Status = {taskInfo.Task.Status} , Url = {taskInfo.Url}");
                }
                Log.Debug("----------------------");
                Log.Debug($"----Image Downloader Running Tasks ({fetcher.TasksRunningQueue.Count()})----");
                foreach ((var taskInfo, var i) in fetcher.TasksRunningQueue.Select((x, i) => (x, i)))
                {
                    Log.Debug($"{i} : Task Status = {taskInfo.Task.Status} , Bytes = {taskInfo.DownloadedBytes}/{taskInfo.TotalContentBytes} ({taskInfo.DownloadedBytes * 100.0 / taskInfo.TotalContentBytes:F2}%) , Url = {taskInfo.Url}");
                }
                Log.Debug("----------------------");
            }
        }

        private static void OnSchedulers(ConsoleWindow.Command command)
        {
            var manager = Container.Get<ISchedulerManager>();
            if (command.Args.Any(x => x.Name.ToLower() == "list"))
            {
                Log.Debug("----Scheduler List----");
                foreach (var scheduler in manager.CurrentRunningSchedulers)
                    Log.Debug($"Name : {scheduler.SchedulerName} \t Interval : {scheduler.ScheduleCallLoopInterval}");
                Log.Debug("----------------------");
            }
            else if (command.Args.FirstOrDefault(x => x.Name.ToLower() == "remove") is CommandArg removeArg)
            {
                if (string.IsNullOrWhiteSpace(removeArg.Value))
                {
                    Log.Debug($"Remove scheduler name is empty");
                    return;
                }

                if (manager.CurrentRunningSchedulers.FirstOrDefault(x => x.SchedulerName == removeArg.Value) is not ISchedulable scheduler)
                {
                    Log.Debug($"Remove scheduler name \"{removeArg.Value}\" is not found.");
                    return;
                }

                manager.RemoveScheduler(scheduler);
                Log.Debug($"Remove scheduler name \"{removeArg.Value}\" successfully.");
            }
        }

        private static void OnConsole(ConsoleWindow.Command command)
        {
            if (command.Args.FirstOrDefault(x => x.Name.ToLower() == "autoclear") is CommandArg arg)
            {
                autoClearConsole = bool.TryParse(arg.Value, out var d) ? d : true;
                Log.Debug($"autoClearConsole = {autoClearConsole}");
            }
            if (command.Args.FirstOrDefault(x => x.Name.ToLower() == "enablelog") is CommandArg arg2)
            {
                Log.EnableConsoleOutputLog = bool.TryParse(arg2.Value, out var d) ? d : true;
                Log.Debug($"Log.EnableConsoleOutputLog = {Log.EnableConsoleOutputLog}");
            }
            if (command.Args.Any(x => x.Name.ToLower() == "hide"))
            {
                ConsoleWindow.Hide();
            }
        }
    }
}
