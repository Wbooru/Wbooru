using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wbooru.Models.Gallery;
using Wbooru.Galleries;
using Wbooru.Settings;
using Wbooru.Network;
using System.Windows.Media.Animation;
using Wbooru.UI.Controls;
using Wbooru.Utils;
using Wbooru.Kernel;
using Wbooru.Galleries.SupportFeatures;
using Wbooru.Models;
using Wbooru.Persistence;
using Wbooru.UI.Controls.PluginExtension;
using Microsoft.EntityFrameworkCore;

namespace Wbooru.UI.Pages
{
    using Logger = Log<MainGalleryPage>;

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainGalleryPage : Page
    {
        public enum GalleryViewType
        {
            Marked, Voted, Main, SearchResult , History
        }

        public Gallery CurrentGallery
        {
            get { return (Gallery)GetValue(CurrentGalleryProperty); }
            set { SetValue(CurrentGalleryProperty, value); }
        }

        public string GalleryTitle
        {
            get { return (string)GetValue(GalleryTitleProperty); }
            set { SetValue(GalleryTitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GalleryTitleProperty =
            DependencyProperty.Register("GalleryTitle", typeof(string), typeof(MainGalleryPage), new PropertyMetadata(""));

        public static readonly DependencyProperty CurrentGalleryProperty =
            DependencyProperty.Register("CurrentGallery", typeof(Gallery), typeof(MainGalleryPage), new PropertyMetadata(null));

        public GlobalSetting Setting { get; private set; }
        public ImageFetchDownloadScheduler ImageDownloader { get; private set; }

        public bool ShowReturnButton
        {
            get { return (bool)GetValue(ShowReturnButtonProperty); }
            set { SetValue(ShowReturnButtonProperty, value); }
        }

        public IEnumerable<string> Keywords { get; }

        // Using a DependencyProperty as the backing store for ShowReturnButton.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowReturnButtonProperty =
            DependencyProperty.Register("ShowReturnButton", typeof(bool), typeof(MainGalleryPage), new PropertyMetadata(false));

        public MainGalleryPage(IEnumerable<string> keywords=null,Gallery lock_gallery = null)
        {
            Keywords = keywords;

            InitializeComponent();

            Setting = SettingManager.LoadSetting<GlobalSetting>();

            GridViewer.GridItemWidth = Setting.PictureGridItemWidth;
            GridViewer.GridItemMarginWidth = Setting.PictureGridItemMarginWidth;

            DataContext = this;

            try
            {
                var galleries = Container.Default.GetExportedValues<Gallery>();

                if (Setting.EnableNSFWFileterMode)
                {
                    Log.Info("EnableNSFWFileterMode = true , skip galleries which not support NSFWFilter in main gallery page.");
                    galleries = galleries.ToArray().Where(x => x.SupportFeatures.HasFlag(GallerySupportFeature.NSFWFilter));
                }

                var gallery = galleries.FirstOrDefault();
                ImageDownloader = Container.Default.GetExportedValue<ImageFetchDownloadScheduler>();

                if (lock_gallery == null)
                {
                    var list = galleries.ToList();
                    var i = Math.Max(0, list.IndexOf(list.FirstOrDefault(x => x.GalleryName == SettingManager.LoadSetting<GlobalSetting>().RememberLastViewedGalleryName)));
                    GalleriesSelector.ItemsSource = list;
                    GalleriesSelector.SelectionChanged += GalleriesSelector_SelectionChanged;
                    GalleriesSelector.SelectedIndex = i;

                    if (list.Count>i)
                        ApplyGallery(list[i], Keywords);

                    if (galleries.Count() <= 1)
                        GalleriesSelector.Visibility = Visibility.Hidden;
                }
                else
                {
                    GalleriesSelector.Visibility = Visibility.Hidden;
                    ApplyGallery(lock_gallery, Keywords);
                }

                if (gallery == null)
                    EmptyImageSourceNotify.Visibility = Visibility.Visible;
            }
            catch (Exception e)
            {
                Logger.Warn("Failed to get a gallery.:" + e.Message);
            }

            if (keywords == null)
                TryAddExtraContent();
        }

        private void TryAddExtraContent()
        {
            //menu items
            var menu_items = Container.Default.GetExportedValues<IExtraMainMenuItemCreator>().Select(x => x.Create());
            var current_items = MainMenu.Children.OfType<UIElement>().ToArray();

            foreach (var item in menu_items.Where(x => !current_items.Any(y => y.GetValue(NameProperty) == x.GetValue(NameProperty))))
                MainMenu.Children.Add(item);
        }

        public async void ApplyGallery(Gallery gallery,IEnumerable<string> keywords=null)
        {
            Func<IEnumerable<GalleryItem>> items_source_creator;

            CurrentGallery = gallery;
            SettingManager.LoadSetting<GlobalSetting>().RememberLastViewedGalleryName = gallery.GalleryName;

            GridViewer.ClearGallery();
            GridViewer.Gallery = gallery;

            if (keywords?.Any() ?? false)
            {
                GridViewer.ViewType = GalleryViewType.SearchResult;
                items_source_creator = new Func<IEnumerable<GalleryItem>>(() => gallery.TryFilterIfNSFWEnable(gallery.Feature<IGallerySearchImage>().SearchImages(keywords).MakeMultiThreadable()));
                GalleryTitle = $"{gallery.GalleryName} ({string.Join(" ", keywords)})";
                ShowReturnButton = true;
            }
            else
            {
                GridViewer.ViewType = GalleryViewType.Main;
                items_source_creator = new Func<IEnumerable<GalleryItem>>(() => gallery.TryFilterIfNSFWEnable(gallery.GetMainPostedImages().MakeMultiThreadable()));
                GalleryTitle = gallery.GalleryName;
                ShowReturnButton = false;
            }

            var status = LoadStatusDisplayer.BeginBusy("正在自动登录账号...");

            if (gallery.SupportFeatures.HasFlag(GallerySupportFeature.Account))
                await TryAutoLogin(gallery);

            status.Dispose();

            UpdateAccountButtonText();

            GridViewer.LoadableSource = items_source_creator;

            Log.Info($"Switch main page gallery:{gallery.GalleryName}");
        }

        private async Task TryAutoLogin(Gallery gallery)
        {
            AccountButton.IsBusy = true;
            AccountButton.Text = "登录中...";

            if (AccountInfoDataContainer.Instance.TryGetAccountInfoData(gallery) is AccountInfo account_info)
            {
                var login = gallery.Feature<IGalleryAccount>();

                try
                {
                    await Task.Run(() => login.AccountLogin(account_info));

                    Log.Info($"Auto login gallery {gallery.GalleryName} -> {login.IsLoggined}");
                }
                catch (Exception e)
                {
                    Log.Info($"Auto login gallery failed:{e.Message}");
                    Toast.ShowMessage($"自动登录{gallery.GalleryName}失败，原因:{e.Message}");
                }
            }

            AccountButton.IsBusy = false;
        }

        #region Left Menu Show/Hide

        private void CloseLeftPanel()
        {
            var hide_sb = Resources["HideLeftPane"] as Storyboard;
            hide_sb.Begin(MainGrid);
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            var show_sb = Resources["ShowLeftPane"] as Storyboard;

            show_sb.Begin(MainGrid);
        }

        bool LeftMenuPanel_Enter;

        private void LeftMenuPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            LeftMenuPanel_Enter = true;
            Log.Debug("Mouse entered left menu");

            Task.Run(async () =>
            {
                await Task.Delay(3000);

                if (LeftMenuPanel_Enter)
                    return;

                await Dispatcher.BeginInvoke(new Action(() => {
                    CloseLeftPanel();
                }));
                Log.Debug("Mouse have left menu over 3s,auto close left menu.");
            });
        }

        private void LeftMenuPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            LeftMenuPanel_Enter = false;
            Log.Debug("Mouse leave left menu");
        }

        #endregion

        private void MenuButton_MouseDown(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Close();
        }

        private void GridViewer_ClickItemEvent(GalleryItem item)
        {
            var page = new PictureDetailViewPage();
            NavigationHelper.NavigationPush(page);
            page.ApplyItem(CurrentGallery, item);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigationPop();
        }

        private void SearchInput_SearchRequest(string obj)
        {
            var keywords = obj.Split(' ');

            NavigationHelper.NavigationPush(new MainGalleryPage(keywords, CurrentGallery));
        }

        private void MenuButton_Click_1(object sender, RoutedEventArgs e)
        {
            var page = new SettingPage();

            NavigationHelper.NavigationPush(page);
        }

        private void DownloadPageButton_Click(object sender, RoutedEventArgs e)
        {
            var page = new DownloadListPage();

            NavigationHelper.NavigationPush(page);
        }

        private void TagPanelButton_Click(object sender, RoutedEventArgs e)
        {
            var show_sb = Resources["ShowRightPane"] as Storyboard;

            show_sb.Begin(TagListViewerPanel);
        }

        private void TagListViewer_CloseTagPanelEvent(TagListViewer obj)
        {
            var show_sb = Resources["HideRightPane"] as Storyboard;

            show_sb.Begin(TagListViewerPanel);
        }

        private void CloseLeftPanelButton_Click(object sender, RoutedEventArgs e)
        {
            CloseLeftPanel();
        }

        LoadingTaskNotify item_loading_notify;

        private void GridViewer_OnRequestMoreItemStarted(GalleryGridView obj)
        {
            item_loading_notify?.Dispose();
            item_loading_notify = LoadStatusDisplayer.BeginBusy("正在加载图片列表...");
        }

        private void GridViewer_OnRequestMoreItemFinished(GalleryGridView obj)
        {
            item_loading_notify?.Dispose();
            item_loading_notify = null;
        }

        private async void ShowMarkPicturesButton_Click(object sender, RoutedEventArgs e)
        {
            if (GridViewer.ViewType == GalleryViewType.Marked)
                return;

            var galleries = (CurrentGallery != null ? new[] { CurrentGallery } : Container.Default.GetExportedValues<Gallery>()).Select(x=>x.GalleryName).ToArray();

            var online_mark_feature = CurrentGallery?.Feature<IGalleryMark>();
            Func<IEnumerable<GalleryItem>> source = null;

            if (online_mark_feature != null)
            {
                source = new Func<IEnumerable<GalleryItem>>(() => online_mark_feature?.GetMarkedGalleryItem());
            }
            else
            {
                var collection = await LocalDBContext.PostDbAction(ctx => ctx.ItemMarks.Include(x => x.GalleryItem)
                .Select(x => x.GalleryItem)
                .Where(x => galleries.Contains(x.GalleryName))
                .ToArray()//avoid SQL.
                );
                source = new Func<IEnumerable<GalleryItem>>(() => collection);
            }
                 
            GalleryTitle = (CurrentGallery != null ? $"{CurrentGallery.GalleryName}的" : "") + (online_mark_feature!=null?"在线":"本地") + "收藏列表";
            GridViewer.ViewType = GalleryViewType.Marked;
            GridViewer.ClearGallery();
            GridViewer.Gallery = null;
            GridViewer.LoadableSource = source;

            CloseLeftPanel();
        }

        private void ShowPicturePoolButton_Click(object sender, RoutedEventArgs e)
        {
            if (GridViewer.ViewType == GalleryViewType.Main)
                return;

            ApplyGallery(CurrentGallery, null);

            CloseLeftPanel();
        }

        private void ShowTagManagePageButton_Click(object sender, RoutedEventArgs e)
        {
            var page = new TagManagePage();

            NavigationHelper.NavigationPush(page);
        }

        private async void AccountButton_Click(object sender, RoutedEventArgs e)
        {
            AccountButton.IsBusy = true;

            var feature = CurrentGallery.Feature<IGalleryAccount>();

            if (feature.IsLoggined)
            {
                AccountButton.BusyStatusDescription = "正在登出中...";

                await Task.Run(() => feature.AccountLogout());
                
                UpdateAccountButtonText();
                AccountInfoDataContainer.Instance.CleanAccountInfo(CurrentGallery);
                Toast.ShowMessage("登出成功");
                CloseLeftPanel();
            }
            else
            {
                AccountButton.BusyStatusDescription = "正在登入中...";
                await Task.Run(() => DoLogin());
            }

            AccountButton.IsBusy = false;
            AccountButton.BusyStatusDescription = string.Empty;
        }

        private void UpdateAccountButtonText()
        {
            AccountButton.Text = CurrentGallery?.Feature<IGalleryAccount>()?.IsLoggined ?? false ? "登出" : "登录";
        }

        HashSet<CustomLoginPage> cache_login_page = new HashSet<CustomLoginPage>();

        private void DoLogin()
        {
            Dispatcher.Invoke(() =>
            {
                var feature = CurrentGallery?.Feature<IGalleryAccount>();

                if (feature?.CustomLoginPage is CustomLoginPage page)
                {
                    if (!cache_login_page.Contains(page))
                    {
                        page.Unloaded += (e, d) => UpdateAccountButtonText();
                        cache_login_page.Add(page);
                    }

                    Log.Info($"Show gallery {CurrentGallery?.GalleryName}'s custom login page {page.GetType().Name}.");
                    NavigationHelper.NavigationPush(page);
                }
                else
                {
                    Log.Info($"Show default login page for gallery {CurrentGallery?.GalleryName}.");
                    page = new DefaultLoginPage(CurrentGallery);
                    page.Unloaded += (e, d) => UpdateAccountButtonText();
                    NavigationHelper.NavigationPush(page);
                }
            });
        }

        private void PageJumpLabel_MouseEnter(object sender, MouseEventArgs e)
        {
        }

        private void JumpConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            PageJumpPopup.IsOpen = false;

            if (!int.TryParse(JumpPageInput.Text,out var page))
                return;

            GridViewer.ChangePage(page);
        }

        private void PageJumpLabel_Click(object sender, RoutedEventArgs e)
        {
            DisplayLoadedPageCount.Text = GridViewer.DisplayedLogicPageIndex.ToString();
            PageJumpPopup.IsOpen = true;
        }

        private void PageJumpPopup_MouseLeave(object sender, MouseEventArgs e)
        {
            PageJumpPopup.IsOpen = false;
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            var page = new AboutPage();

            NavigationHelper.NavigationPush(page);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            GridViewer.RefreshItem();
        }

        private void MenuButton_Click_2(object sender, RoutedEventArgs e)
        {
            var page = new PluginManagerPage();

            NavigationHelper.NavigationPush(page);
        }

        private void JumpMarketButton_Click(object sender, RoutedEventArgs e)
        {
            var page = new PluginManagerPage(PluginManagerPage.LayoutState.MarketPart);

            NavigationHelper.NavigationPush(page);
        }

        private void ShowHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (GridViewer.ViewType == GalleryViewType.History)
                return;

            var galleries = (CurrentGallery != null ? new[] { CurrentGallery } : Container.Default.GetExportedValues<Gallery>()).Select(x => x.GalleryName).ToArray();

            using var _ = ObjectPool<LocalDBContext>.GetWithUsingDisposable(out var ctx,out var __);

            var source = new Func<IEnumerable<GalleryItem>>(() => ctx.VisitRecords.AsEnumerable()
            .OrderByDescending(x => x.LastVisitTime)
            .Select(x => x.GalleryItem)
            .Where(x => string.IsNullOrWhiteSpace(galleries.FirstOrDefault(y => y == x.GalleryName)))
            .ToArray()
            );//avoid SQL.

            GalleryTitle = "历史浏览记录";
            GridViewer.ViewType = GalleryViewType.History;
            GridViewer.ClearGallery();
            GridViewer.Gallery = null;
            GridViewer.LoadableSource = source;

            CloseLeftPanel();
        }

        private void GalleriesSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(GalleriesSelector.SelectedItem is Gallery gallery))
                return;

            ApplyGallery(gallery, Keywords);
        }

        private void TagListViewerPanel_RequestSearchEvent(IEnumerable<Tag> tags) => NavigationHelper.NavigationPush(new MainGalleryPage(tags.Select(x => x.Name), CurrentGallery));
    }
}
