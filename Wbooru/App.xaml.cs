using System.ComponentModel.Composition;
using System.Windows;
using Wbooru.Kernel;
using System.Linq;
using Wbooru.Settings;
using System.Collections.Generic;
using Wbooru.Galleries;

namespace Wbooru
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        [Import(typeof(SchedulerManager))]
        public SchedulerManager SchedulerManager { get; set; }

        [Import(typeof(SettingManager))]
        public SettingManager SettingManager { get; set; }

        public App()
        {
            Container.BuildDefault();

            Container.Default.ComposeParts(this);

            DownloadManager.Init();
        }

        ~App ()
        {

        }
    }
}
