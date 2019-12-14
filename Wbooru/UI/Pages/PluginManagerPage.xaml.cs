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
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wbooru.Kernel;
using Wbooru.Kernel.Updater;
using Wbooru.Kernel.Updater.PluginMarket;
using Wbooru.PluginExt;
using Wbooru.Settings;
using Wbooru.Utils;

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

        public PluginManagerPage(LayoutState state = LayoutState.UpdatablePart)
        {
            InitializeComponent();

            PluginMarketPosts = new ObservableCollection<PluginMarketPost>();

            PluginInfoList.ItemsSource = Container.Default.GetExportedValues<PluginInfo>().Select(x => new PluginInfoWrapper()
            {
                PluginInfo = x,
                UpdatableVersion = PluginUpdaterManager.UpdatablePluginsInfo.TryGetValue(x.GetType(), out var info) ? info?.Version : null
            });

            layout_translate_storyboard = new Storyboard();
            layout_translate_storyboard.Completed += (e, d) =>
            {
                ViewPage_SizeChanged(null, null);
                ObjectPool<ThicknessAnimation>.Return(e as ThicknessAnimation);
            };

            var plugin_markets = Container.Default.GetExportedValues<PluginMarket>();

            PluginMarketList.ItemsSource = plugin_markets;

            MainPanel.DataContext = this;

            MessageList.ItemsSource = new ObservableCollection<string>();

            current_layout = state;
            ApplyTranslate();
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
                StartProgress(LayoutState.UpdatablePart);

                Task.Run(()=>
                PluginUpdaterManager.BeginPluginUpdate(new[] { wrapper.PluginInfo as IPluginUpdatable },msg=> {
                    AppendProgressMessage(msg);
                    Log.Info("BeginPluginUpdate Message:" + msg);
                }));
            }
        }

        private void StartProgress(LayoutState state)
        {
            MarketPanelPart.Children.Remove(UpdatingPanel);
            UpdatePanelPart.Children.Remove(UpdatingPanel);

            switch (state)
            {
                case LayoutState.UpdatablePart:
                    ProgressHeader.Text = "正在更新...";
                    UpdatePanelPart.Children.Add(UpdatingPanel);
                    break;
                case LayoutState.MarketPart:
                    ProgressHeader.Text = "正在下载...";
                    MarketPanelPart.Children.Add(UpdatingPanel);
                    break;
                default:
                    break;
            }

            (MessageList.ItemsSource as ObservableCollection<string>).Clear();
            UpdatingPanel.Visibility = Visibility.Visible;
        }

        private void AppendProgressMessage(string message) => Dispatcher.Invoke(() => (MessageList.ItemsSource as ObservableCollection<string>).Add(message));

        private void EndProgress()
        {
            UpdatingPanel.Visibility = Visibility.Hidden;
        }

        public void OnNavigationBackAction()
        {
            if (UpdatingPanel.Visibility == Visibility.Visible)
                return;

            switch (current_layout)
            {
                case LayoutState.UpdatablePart:
                    NavigationHelper.NavigationPop();
                    break;
                case LayoutState.MarketPart:
                    MenuButton_Click_2(null, null);
                    break;
                default:
                    break;
            }
        }

        public void OnNavigationForwardAction()
        {
            if (UpdatingPanel.Visibility == Visibility.Visible)
                return;

            switch (current_layout)
            {
                case LayoutState.UpdatablePart:
                    MenuButton_Click_1(null, null);
                    break;
                default:
                    break;
            }
        }

        public enum LayoutState
        {
            UpdatablePart, MarketPart
        }

        LayoutState current_layout = LayoutState.UpdatablePart;
        private Storyboard layout_translate_storyboard;

        private void MenuButton_Click_1(object sender, RoutedEventArgs e)
        {
            current_layout = LayoutState.MarketPart;
            ApplyTranslate();
        }

        private void MenuButton_Click_2(object sender, RoutedEventArgs e)
        {
            current_layout = LayoutState.UpdatablePart;
            ApplyTranslate();
        }

        private Thickness CalculateMargin()
        {
            double margin_left = 0;

            switch (current_layout)
            {
                case LayoutState.UpdatablePart:
                    margin_left = 0;
                    break;
                case LayoutState.MarketPart:
                    margin_left = 1;
                    break;
                default:
                    break;
            }

            margin_left *= -ViewPage.ActualWidth;

            return new Thickness(margin_left, 0, 0, 0);
        }

        private void ApplyTranslate()
        {
            layout_translate_storyboard.Children.Clear();

            if (ObjectPool<ThicknessAnimation>.Get(out var animation))
            {
                //init 
                animation.Duration = new Duration(TimeSpan.FromMilliseconds(250));
                animation.FillBehavior = FillBehavior.Stop;
                Storyboard.SetTargetProperty(animation, new PropertyPath(Grid.MarginProperty));
                animation.EasingFunction = animation.EasingFunction ?? new QuadraticEase() { EasingMode = EasingMode.EaseOut };
            }

            animation.To = CalculateMargin();

            layout_translate_storyboard.Children.Clear();
            layout_translate_storyboard.Children.Add(animation);
            layout_translate_storyboard.Begin(MainPanel);
        }

        private void ViewPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var new_margin = CalculateMargin();
            MainPanel.Margin = new_margin;
        }

        public ObservableCollection<PluginMarketPost> PluginMarketPosts
        {       
            get { return (ObservableCollection<PluginMarketPost>)GetValue(PluginMarketPostsProperty); }
            set { SetValue(PluginMarketPostsProperty, value); }
        }

        public static readonly DependencyProperty PluginMarketPostsProperty =
            DependencyProperty.Register("PluginMarketPosts", typeof(ObservableCollection<PluginMarketPost>), typeof(PluginManagerPage), new PropertyMetadata(null));

        private void PluginMarketList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PluginMarketList.IsEnabled = false;

            if (PluginMarketList.SelectedItem is PluginMarket market)
            {
                Task.Run(() =>
                {
                    using (var status = MarketStatusDisplayer.BeginBusy("正在获取插件列表"))
                    {
                        var plugin_list = market.GetPluginPosts().ToArray();

                        Dispatcher.Invoke(() =>
                        {
                            PluginMarketList.IsEnabled = true;
                            PluginMarketPosts.Clear();

                            foreach (var item in plugin_list)
                                PluginMarketPosts.Add(item);
                        });
                    }
                });
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var post = (sender as FrameworkElement).DataContext as PluginMarketPost;
            StartProgress(LayoutState.MarketPart);

            var releases = (post.ReleaseInfos ?? Enumerable.Empty<PluginMarketRelease>()).OrderBy(x=>x.Version);

            var pick = SettingManager.LoadSetting<GlobalSetting>().UpdatableTargetVersion == GlobalSetting.UpdatableTarget.Preview ?
                releases.FirstOrDefault() : releases.Where(x => x.ReleaseType != ReleaseType.Preview).FirstOrDefault();

            if (pick==null)
            {
                AppendProgressMessage("没发现任何可以下载的版本,三秒后自动关闭此页面...");
                Task.Delay(3000).ContinueWith(_ => Dispatcher.Invoke(() => EndProgress()));
                return;
            }

            AppendProgressMessage($"选择 : {pick.Version} {pick.ReleaseType} {pick.ReleaseDate} {pick.DownloadURL} {pick.ReleaseURL}");
            AppendProgressMessage($"开始下载 : {pick.DownloadURL}");

            Task.Run(() =>
            {
                PluginUpdaterManager.InstallPluginRelease(pick, msg => AppendProgressMessage(msg));
            });
        }
    }
}
