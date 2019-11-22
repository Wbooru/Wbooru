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
            var args = Environment.GetCommandLineArgs();

            //check if it need finish updating.
            if (args.Where(x => x == "-update").Any())
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
        }

        internal static void UnusualSafeExit()
        {
            Term();
            Current.Shutdown();
        }
    }
}
