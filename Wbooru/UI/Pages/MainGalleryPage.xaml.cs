using System;
using System.Collections.Generic;
using System.IO;
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
using System.Drawing;
using Wbooru.Models.Gallery;
using Wbooru.Galleries;
using Wbooru.Settings;
using Wbooru.Utils.Resource;
using Wbooru.Network;
using System.Windows.Media.Animation;
using Wbooru.UI.Controls;
using Wbooru.Utils;
using Wbooru.Kernel;
using Wbooru.Galleries.SupportFeatures;
using System.Collections.ObjectModel;
using Wbooru.Models;
using Wbooru.Persistence;

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
            Marked, Voted, Main, SearchResult
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

        // Using a DependencyProperty as the backing store for ShowReturnButton.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowReturnButtonProperty =
            DependencyProperty.Register("ShowReturnButton", typeof(bool), typeof(MainGalleryPage), new PropertyMetadata(false));

        public MainGalleryPage(IEnumerable<string> keywords=null)
        {
            InitializeComponent();

            DataContext = this;

            try
            {
                var gallery = Container.Default.GetExportedValue<Gallery>();
                Setting = SettingManager.LoadSetting<GlobalSetting>();
                ImageDownloader = Container.Default.GetExportedValue<ImageFetchDownloadScheduler>();

                if (gallery != null)
                    ApplyGallery(gallery, keywords);
            }
            catch (Exception e)
            {
                Logger.Warn("Failed to get a gallery.:" + e.Message);
            }
        }

        public void ApplyGallery(Gallery gallery,IEnumerable<string> keywords=null)
        {
            IEnumerable<GalleryItem> items_source;

            if (keywords?.Any() ?? false)
            {
                GridViewer.ViewType = GalleryViewType.SearchResult;
                items_source = gallery.Feature<IGallerySearchImage>().SearchImages(keywords).MakeMultiThreadable();
                GalleryTitle = $"{gallery.GalleryName} ({string.Join(" ", keywords)})";
                ShowReturnButton = true;
            }
            else
            {
                GridViewer.ViewType = GalleryViewType.Main;
                items_source = gallery.GetMainPostedImages().MakeMultiThreadable();
                GalleryTitle = gallery.GalleryName;
                ShowReturnButton = false;
            }

            CurrentGallery = gallery;

            GridViewer.ClearGallery();
            GridViewer.Gallery = gallery;
            GridViewer.LoadableSource = items_source;
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
            var page = ObjectPool<PictureDetailViewPage>.Get();
            page.ApplyItem(CurrentGallery, item);

            NavigationHelper.NavigationPush(page);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigationPop();
        }

        private void SearchInput_SearchRequest(string obj)
        {
            var keywords = obj.Split(' ');

            NavigationHelper.NavigationPush(new MainGalleryPage(keywords));
        }

        private void MenuButton_Click_1(object sender, RoutedEventArgs e)
        {
            var page = new SettingPage();

            NavigationHelper.NavigationPush(page);
        }

        private void MenuButton_Click_2(object sender, RoutedEventArgs e)
        {

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

        private void ShowMarkPicturesButton_Click(object sender, RoutedEventArgs e)
        {
            var galleries = (CurrentGallery != null ? new[] { CurrentGallery } : Container.Default.GetExportedValues<Gallery>()).Select(x=>x.GalleryName).ToArray();

            var online_mark_feature = CurrentGallery?.Feature<IGalleryMark>();

            var source = (online_mark_feature?.GetMarkedGalleryItem()) ?? LocalDBContext.Instance.ItemMarks
                .Select(x => new { gallery = galleries.FirstOrDefault(y => y == x.GalleryName), gallery_item = x })
                .Where(x => x.gallery != null)
                .ToArray()//avoid SQL.
                .Select(x => x.gallery_item.Item.ConvertToNormalModel());
                 

            GalleryTitle = (CurrentGallery != null ? $"{CurrentGallery.GalleryName}的" : "") + (online_mark_feature!=null?"在线":"本地") + "收藏列表";
            GridViewer.ViewType = GalleryViewType.Marked;
            GridViewer.ClearGallery();
            GridViewer.Gallery = null;
            GridViewer.LoadableSource = source;

            CloseLeftPanel();
        }

        private void ShowPicturePoolButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyGallery(CurrentGallery, null);

            CloseLeftPanel();
        }
    }
}
