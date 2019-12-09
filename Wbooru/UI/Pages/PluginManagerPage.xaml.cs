using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
using Wbooru.PluginExt;

namespace Wbooru.UI.Pages
{
    /// <summary>
    /// PluginManagerPage.xaml 的交互逻辑
    /// </summary>
    public partial class PluginManagerPage : Page , INavigatableAction
    {
        public class PluginInfoWrapper : INotifyPropertyChanged
        {
            private PluginInfo plugin_info;

            public PluginInfo PluginInfo
            {
                get => plugin_info;
                set
                {
                    plugin_info = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PluginInfo)));
                }
            }

            private Version updatable_version;

            public Version UpdatableVersion
            {
                get => updatable_version;
                set
                {
                    updatable_version = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UpdatableVersion)));
                }
            }

            private bool is_updatable_checking;

            public bool IsUpdatableChecking
            {
                get => is_updatable_checking;
                set
                {
                    is_updatable_checking = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsUpdatableChecking)));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        public PluginManagerPage()
        {
            InitializeComponent();

            PluginInfoList.ItemsSource = Container.Default.GetExportedValues<PluginInfo>().Select(x => new PluginInfoWrapper()
            {
                PluginInfo = x,
                UpdatableVersion = PluginUpdaterManager.UpdatablePluginsInfo.TryGetValue(x.GetType(), out var info) ? info?.Version : null
            });
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigationPop();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void CheckPluginUpdatable(PluginInfoWrapper info)
        {
            if (info.IsUpdatableChecking)
                return;

            Task.Run(() => {
                info.IsUpdatableChecking = true;
                info.UpdatableVersion = null;

                if (!(PluginUpdaterManager.UpdatablePluginsInfo.TryGetValue(info.PluginInfo.GetType(),out var release_info) && release_info!=null))
                {
                    PluginUpdaterManager.CheckPluginUpdatable(info.PluginInfo as IPluginUpdatable);
                    PluginUpdaterManager.UpdatablePluginsInfo.TryGetValue(info.PluginInfo.GetType(), out release_info);
                }

                info.UpdatableVersion = release_info?.Version;

                info.IsUpdatableChecking = false;
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as FrameworkElement)?.DataContext is PluginInfoWrapper wrapper))
                return;

            if (wrapper.UpdatableVersion == null)
            {
                CheckPluginUpdatable(wrapper);
            }
            else
            {
                UpdatingPanel.Visibility = Visibility.Visible;
                var list =new ObservableCollection<string>();
                MessageList.ItemsSource = list;

                Task.Run(()=>
                PluginUpdaterManager.BeginPluginUpdate(new[] { wrapper.PluginInfo as IPluginUpdatable },msg=> {
                    Dispatcher.Invoke(() =>list.Add(msg));
                    Log.Info("BeginPluginUpdate Message:" + msg);
                }));
            }
        }

        public void OnNavigationBackAction()
        {
            if (UpdatingPanel.Visibility != Visibility.Visible)
                NavigationHelper.NavigationPop();
        }

        public void OnNavigationForwardAction()
        {
        }
    }
}
