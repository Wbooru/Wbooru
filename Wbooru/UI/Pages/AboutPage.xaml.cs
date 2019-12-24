using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wbooru.Kernel;
using Wbooru.Kernel.Updater;

namespace Wbooru.UI.Pages
{
    /// <summary>
    /// AboutPage.xaml 的交互逻辑
    /// </summary>
    public partial class AboutPage : Page, INotifyPropertyChanged
    {
        private ReleaseInfo release_info;

        public event PropertyChangedEventHandler PropertyChanged;

        public ReleaseInfo CacheReleaseInfo
        {
            get => release_info;
            set
            {
                release_info = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CacheReleaseInfo)));
            }
        }

        public AboutPage()
        {
            InitializeComponent();

            MainContent.DataContext = this;

            //apply release info first

            CacheReleaseInfo = ProgramUpdater.CacheUpdatableReleaseInfo;
            CurrentVersionDisplayer.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (CacheReleaseInfo == null)
            {
                //check update
                Task.Run(CheckUpdate);
            }
            else
            {
                Task.Run(StartUpdate);
            }
        }

        private void StartUpdate()
        {
            using (var notify = VersionCheckStatusDispalyer.BeginBusy("正在更新..."))
            {
                ProgramUpdater.BeginUpdate((cur, total) =>
                {
                    notify.Description = $"下载更新({cur * 1.0 / total * 100.0:F2}%)...";
                }, () =>
                {
                    var wait_notify = VersionCheckStatusDispalyer.BeginBusy("即将重启...");
                    return Task.Delay(3000).ContinueWith(_ =>
                    {
                        wait_notify.Dispose();
                        return true;
                    }).Result;
                });
            }
        }

        private void CheckUpdate()
        {
            using (VersionCheckStatusDispalyer.BeginBusy("正在程序是否可更新.."))
            {
                if (ProgramUpdater.CheckUpdatable())
                {
                    Dispatcher.Invoke(() =>
                    {
                        CacheReleaseInfo = ProgramUpdater.CacheUpdatableReleaseInfo;

                        if (CacheReleaseInfo!=null)
                        {

                        }
                    });
                }
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigationPop();
        }
    }
}