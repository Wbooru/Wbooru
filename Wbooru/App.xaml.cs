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

            Init();

            PreprocessCommandLine();
        }

        private void PreprocessCommandLine()
        {
            var args = Environment.GetCommandLineArgs();

            //check if it need finish updating.
            if (CommandLine.ContainSwitchOption("update"))
            {
                ProgramUpdater.ApplyUpdate();
            }
        }

        private void BlockApplicationUntilSingle()
        {
            var cur_process = Process.GetCurrentProcess();

            while (Process.GetProcessesByName(cur_process.ProcessName).Where(x => x.Id != cur_process.Id).Any())
                Thread.Sleep(100);
        }

        internal static void Init()
        {
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
            Current.Shutdown();
        }
    }
}
