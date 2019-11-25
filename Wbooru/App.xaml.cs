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
using Wbooru.Kernel.ProgramUpdater;
using Wbooru.Utils;

namespace Wbooru
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            BlockApplicationUntilSingle();

            PreprocessCommandLine();

            Init();
        }

        private void PreprocessCommandLine()
        {
            //check if it need finish updating.
            if (CommandLine.ContainSwitchOption("update"))
            {
                ProgramUpdater.ApplyUpdate();
            }
        }

        private void BlockApplicationUntilSingle()
        {
            var cur_process = Process.GetCurrentProcess();

            Log.Info("Check&Block Application single instance.......");

            while (Process.GetProcessesByName(cur_process.ProcessName).Where(x => x.Id != cur_process.Id).Any())
                Thread.Sleep(100);

            Log.Info("OK.");
        }

        internal static void Init()
        {
            AppDomain.CurrentDomain.UnhandledException+= (e, d) => Log.Error($"{(d.ExceptionObject as Exception).Message} {Environment.NewLine} {(d.ExceptionObject as Exception).StackTrace}", "UnhandledException");
            Current.DispatcherUnhandledException += (e, d) => Log.Error($"{d.Exception.Message} {Environment.NewLine} {d.Exception.StackTrace}", "UnhandledException");

            Container.BuildDefault();

            DownloadManager.Init();

            SchedulerManager.Init();

            TagManager.InitTagManager();

            ImageResourceManager.InitImageResourceManager();
        }

        internal static void Term()
        {
            DownloadManager.Close();
            SettingManager.SaveSettingFile();
            SchedulerManager.Term();
            Log.Term();
        }

        internal static void UnusualSafeExit()
        {
            Term();
            Environment.Exit(0);
        }
    }
}
