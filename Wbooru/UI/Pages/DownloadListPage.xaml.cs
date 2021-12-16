using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wbooru.Kernel;
using Wbooru.Kernel.DI;
using Wbooru.Models;
using Wbooru.UI.Controls;

namespace Wbooru.UI.Pages
{
    /// <summary>
    /// DownloadListPage.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadListPage : Page
    {
        public DownloadListPage()
        {
            InitializeComponent();

            MainPanel.DataContext = this;

            DownloadList.ItemsSource = Container.Get<IDownloadManager>().DownloadList;
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigationPop();
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if((sender as FrameworkElement).DataContext as DownloadWrapper is DownloadWrapper download_task)
            {
                switch (download_task.Status)
                {
                    case DownloadTaskStatus.Paused:
                        await Container.Get<IDownloadManager>().DownloadStart(download_task);
                        break;
                    case DownloadTaskStatus.Started:
                        await Container.Get<IDownloadManager>().DownloadPause(download_task);
                        break;
                    case DownloadTaskStatus.Finished:
                        ShowRedownloadConfimPanel(sender as Button,download_task);

                        break;
                    default:
                        break;
                }
            }
        }

        private void ShowRedownloadConfimPanel(Button sender, DownloadWrapper download_task)
        {
            var panel = (sender.Parent as FrameworkElement).FindName("RedownloadPanel") as Popup;
            panel.IsOpen = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //just show panel
            var panel = (sender as FrameworkElement).DataContext as Popup;
            panel.IsOpen = true;
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var panel = (sender as FrameworkElement).DataContext as Popup;
            panel.IsOpen = false;

            var need_delete = (panel.FindName("DeleteFileCheckBox") as CheckBox)?.IsChecked??false;

            var download_task = panel.DataContext as DownloadWrapper;

            await Container.Get<IDownloadManager>().DownloadDelete(download_task);

            if (need_delete)
                File.Delete(download_task.DownloadInfo.DownloadFullPath);

            Toast.ShowMessage("已删除");
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var panel = (sender as FrameworkElement).DataContext as Popup;
            panel.IsOpen = false;
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            var panel = (sender as FrameworkElement).DataContext as Popup;
            panel.IsOpen = false;
        }

        private async void Button_Click_5(object sender, RoutedEventArgs e)
        {
            var panel = (sender as FrameworkElement).DataContext as Popup;
            panel.IsOpen = false;
            var download_task = panel.DataContext as DownloadWrapper;

            await Container.Get<IDownloadManager>().DownloadRestart(download_task);
        }
    }
}
