using System.ComponentModel.Composition;
using System.Windows;
using Wbooru.Kernel;
using System.Linq;
using Wbooru.Settings;
using System.Collections.Generic;
using Wbooru.Galleries;
using System;
using Wbooru.Utils.Resource;
using System.Diagnostics;
using System.Threading;
using Wbooru.Kernel.Updater;
using Wbooru.Utils;
using System.IO;
using System.Threading.Tasks;
using Wbooru.PluginExt;
using Wbooru.Kernel.Updater.PluginMarket;
using Wbooru.Persistence;
using Wbooru.Kernel.DI;

namespace Wbooru
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private static bool? _cache_design_mode = null;
        public static bool IsInDesignMode => (_cache_design_mode ?? (_cache_design_mode = System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()))) ?? false;

        public App()
        {
            Log.Info("Enter App()");

            BlockApplicationUntilSingle();

            PreprocessCommandLine();

            Init();

            BeginCheckUpdatable();

            Log.Info("Finish App()");
        }

        private async void BeginCheckUpdatable()
        {
            Log.Info("Enter BeginCheckUpdatable()");
            await Task.Run(() =>
            {
                if (SettingManager.LoadSetting<GlobalSetting>().EnableAutoCheckUpdatable)
                {
                    ProgramUpdater.CheckUpdatable();

                    foreach (var updatable in Container.Default.GetExportedValues<PluginInfo>().OfType<IPluginUpdatable>())
                        PluginUpdaterManager.CheckPluginUpdatable(updatable);
                }
            });
        }

        private void PreprocessCommandLine()
        {
            Log.Info("Enter PreprocessCommandLine()");
            //check if it need finish updating.
            if (CommandLineHelper.ContainSwitchOption("update"))
            {
                ProgramUpdater.ApplyUpdate();
            }

            if (CommandLineHelper.ContainSwitchOption("update_plugin"))
            {
                PluginUpdaterManager.ApplyPluginUpdate();
            }

            if (CommandLineHelper.ContainSwitchOption("database_backup"))
            {
                if (CommandLineHelper.TryGetOptionValue("to",out string to))
                {
                    LocalDBContext.BackupDatabase(to);
                    Environment.Exit(0);
                }
            }

            if (CommandLineHelper.ContainSwitchOption("database_restore"))
            {
                if (CommandLineHelper.TryGetOptionValue("to", out string to) && CommandLineHelper.TryGetOptionValue("from", out string from))
                {
                    LocalDBContext.RestoreDatabase(from,to);
                    Environment.Exit(0);
                }
            }
        }

        private void BlockApplicationUntilSingle()
        {
            Log.Info("Enter BlockApplicationUntilSingle()");
            var cur_process = Process.GetCurrentProcess();

            Log.Info("Check&Block Application single instance.......");
            Log.Debug($"Current process info : {cur_process.Id}/{cur_process.SessionId} - {cur_process.ProcessName}");

            var time = DateTime.Now;

            while (true)
            {
                var processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(ProgramUpdater.EXE_NAME)).Concat(Process.GetProcessesByName(Path.GetFileNameWithoutExtension(ProgramUpdater.UPDATE_EXE_NAME))).Where(x => x.Id != cur_process.Id);

                if (!processes.Any())
                    break;

                if ((DateTime.Now - time).TotalSeconds > -1)
                {
                    Log.Debug($"Current other instances pid:{string.Join(" ", processes.Select(x => x.Id))}");
                    time = DateTime.Now;
                }

                Thread.Sleep(100);
            }

            Log.Info("OK.");
        }

        public static async void Init()
        {
            Log.Info("-----------------Begin Init()-----------------");

#if !DEBUG

            AppDomain.CurrentDomain.UnhandledException += (e, d) =>
             {
                 Log.Error($"{(d.ExceptionObject as Exception).Message} {Environment.NewLine} {(d.ExceptionObject as Exception).StackTrace}", "UnhandledException");
                 FatalAlert();
             };

            if (Current != null)
                Current.DispatcherUnhandledException += (e, d) =>
                {
                    Log.Error($"{d?.Exception?.Message} {Environment.NewLine} {d?.Exception?.StackTrace}", "UnhandledException");
                    FatalAlert();
                };
            
#endif
            Log.Info("Program version:" + ProgramUpdater.CurrentProgramVersion.ToString());

            if (LocalDBContext.CheckIfUsingOldDatabase())
            {
                Log.Warn("Program is using old version db file and its db file have to upgrade.");

                if (MessageBoxResult.OK == MessageBox.Show($"检查到Wbooru正在使用的数据库文件是老版本的，即将对此数据库文件进行更新以及数据迁移,请备份好数据库文件并点击确认开始:{Path.GetFullPath(Setting<GlobalSetting>.Current.DBFilePath)}","警告",MessageBoxButton.OKCancel))
                {
                    if (await LocalDBContext.UpdateOldDatabase())
                    {
                        //migrate successfully
                        MessageBox.Show("数据库迁移升级成功!");
                        UnusualSafeRestart();
                    }
                    else
                    {
                        try
                        {
                            Process.Start(Log.LogFilePath);
                        }
                        catch {
                            MessageBox.Show("数据库迁移升级失败!查看日志获取更详细情况.");
                            UnusualSafeRestart();
                        }
                    }
                }
                else
                {
                    UnusualSafeExit();
                }
            }

            Container.BuildDefault();

            CheckPlugin();

            SchedulerManager.Init();

            await Singleton<IDownloadManager>.Instance.OnInit();

            await TagManager.InitTagManager();

            ImageResourceManager.InitImageResourceManager();

            Log.Info("-----------------End Init()-----------------");
        }

        static volatile object fatal_locker = new object();
        private static void FatalAlert()
        {
            lock (fatal_locker)
            {
                //skip if it's in design mode
                if (IsInDesignMode)
                    return;

                Process.Start(Log.LogFilePath);
                MessageBox.Show("Wbooru遇到了无法解决的错误，程序即将关闭。请查看日志文件.", "Wbooru",MessageBoxButton.OK,MessageBoxImage.Error);

                Environment.Exit(0);
            }
        }

        private static void CheckPlugin()
        {
            var conflict_plugin_group = Container.Default.GetExportedValues<PluginInfo>().GroupBy(x => x.PluginName).Where(x => x.Count() > 1);

            foreach (var p in conflict_plugin_group)
            {
                var message = $"There contains plugin conflict that plugin name is same:\"{p.Key}\" : {Environment.NewLine} {string.Join(Environment.NewLine, p.Select(x => x.GetType().Assembly.Location).Select(x => $" -- {x}"))}";
                Log.Error(message);

                MessageBox.Show(message,"Wbooru.CheckPlugin()");
            }

            if (conflict_plugin_group.Any())
            {
                UnusualSafeExit();
            }
        }

        internal static async Task Term()
        {
            Log.Info("-----------------Begin Term()-----------------");
            SafeTermSubModule(PluginsTerm);
            await SafeTermSubModuleAsync(Singleton<IDownloadManager>.Instance.OnExit);
            SafeTermSubModule(SettingManager.SaveSettingFile);
            SafeTermSubModule(SchedulerManager.Term);
            SafeTermSubModule(Log.Term);
            Log.Info("-----------------End Term()-----------------");

            async Task SafeTermSubModuleAsync(Func<Task> action)
            {
                try
                {
                    await action();
                }
                catch (Exception e)
                {
                    ExceptionHelper.DebugThrow(e);
                }
            }

            void SafeTermSubModule(Action action)
            {
                try
                {
                    action?.Invoke();
                }
                catch (Exception e)
                {
                    ExceptionHelper.DebugThrow(e);
                }
            }
        }

        private static void PluginsTerm()
        {
            Log.Info("Call PluginsTerm()");

            foreach (var plugin in Container.Default.GetExportedValues<PluginInfo>())
            {
                Log.Info($"Call {plugin.PluginName}.OnApplicationTerm()");
                plugin.CallApplicationTerm();
            }
        }

        internal static async void UnusualSafeExit()
        {
            Log.Info("Begin save&clean program data/resources");

            try
            {
                await Term();
            }
            catch (Exception e)
            {
                Log.Error($"Term() thrown exception:{e.Message}{Environment.NewLine}{e.StackTrace}");
            }
            finally
            {
                Environment.Exit(0);
            }
        }

        internal static async void UnusualSafeRestart()
        {
            try
            {
                await Term();
            }
            catch (Exception e)
            {
                ExceptionHelper.DebugThrow(e);
            }
            finally
            {
                Process.Start(Process.GetCurrentProcess().MainModule.FileName);
                Environment.Exit(0);
            }
        }
    }
}
