using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wbooru.Galleries;
using Wbooru.Galleries.SupportFeatures;
using Wbooru.Kernel;
using Wbooru.Models;
using Wbooru.Models.Gallery;
using Wbooru.Network;
using Wbooru.Persistence;
using Wbooru.Settings;
using Wbooru.UI.Controls;
using Wbooru.UI.Dialogs;
using Wbooru.Utils;
using Wbooru.Utils.Resource;
using static Wbooru.Models.TagRecord;
using Brushes = System.Windows.Media.Brushes;

namespace Wbooru.UI.Pages
{
    /// <summary>
    /// PictureDetailViewPage.xaml 的交互逻辑
    /// </summary>
    public partial class PictureDetailViewPage : Page , INavigatableAction
    {
        public Gallery Gallery
        {
            get { return (Gallery)GetValue(GalleryProperty); }
            set { SetValue(GalleryProperty, value); }
        }

        public static readonly DependencyProperty GalleryProperty =
            DependencyProperty.Register("Gallery", typeof(Gallery), typeof(PictureDetailViewPage), new PropertyMetadata(null));

        public GalleryItem PictureInfo
        {
            get { return (GalleryItem)GetValue(PictureInfoProperty); }
            set { SetValue(PictureInfoProperty, value); }
        }

        public static readonly DependencyProperty PictureInfoProperty =
            DependencyProperty.Register("PictureInfo", typeof(GalleryItem), typeof(PictureDetailViewPage), new PropertyMetadata(null));

        public GalleryImageDetail PictureDetailInfo
        {
            get { return (GalleryImageDetail)GetValue(PictureDetailInfoProperty); }
            set { SetValue(PictureDetailInfoProperty, value); }
        }

        public static readonly DependencyProperty PictureDetailInfoProperty =
            DependencyProperty.Register("PictureDetailInfo", typeof(GalleryImageDetail), typeof(PictureDetailViewPage), new PropertyMetadata(null, (e, d) => (e as PictureDetailViewPage)?.ChangeDetailPicture(d.NewValue as GalleryImageDetail)));

        public bool IsMark
        {
            get { return (bool)GetValue(IsMarkProperty); }
            set { SetValue(IsMarkProperty, value); }
        }

        public static readonly DependencyProperty IsMarkProperty =
            DependencyProperty.Register("IsMark", typeof(bool), typeof(PictureDetailViewPage),
                new PropertyMetadata(false));

        public bool IsVoted
        {
            get { return (bool)GetValue(IsVotedProperty); }
            set { SetValue(IsVotedProperty, value); }
        }

        public static readonly DependencyProperty IsVotedProperty =
            DependencyProperty.Register("IsVoted", typeof(bool), typeof(PictureDetailViewPage),
                new PropertyMetadata(false));

        public LocalDBContext DB { get; }

        public DownloadableImageLink CurrentDisplayImageLink { get; private set; }

        static PictureDetailViewPage()
        {
            ObjectPool<ThicknessAnimation>.EnableTrim = false;
        }

        public PictureDetailViewPage()
        {
            InitializeComponent();

            DB = LocalDBContext.Instance;

            MainGrid.DataContext = this;

            layout_translate_storyboard = new Storyboard();
            layout_translate_storyboard.Completed += (e, d) =>
            {
                ViewPage_SizeChanged(null, null);
                ObjectPool<ThicknessAnimation>.Return(e as ThicknessAnimation);
            };
        }

        private async void ChangeDetailPicture(GalleryImageDetail galleryImageDetail)
        {
            if (galleryImageDetail == null)
                return;

            var pick_download = PickSuitableImageURL(galleryImageDetail.DownloadableImageLinks);

            if (pick_download == null)
            {
                //notice error;
                ExceptionHelper.DebugThrow(new Exception("No image."));
                Toast.ShowMessage("没图片可显示");
                return;
            }

            await DisplayImage(pick_download);

        }

        private async Task DisplayImage(DownloadableImageLink pick_download)
        {
            DetailImageBox.ImageSource = null;
            RefreshButton.IsBusy = true;

            const string notify_content = "加载图片中......";

            using var notify = LoadingStatus.BeginBusy(notify_content);

            var downloader = Container.Default.GetExportedValue<ImageFetchDownloadScheduler>();

            System.Drawing.Image image;

            do
            {
                image = await ImageResourceManager.RequestImageAsync(pick_download.FullFileName, async () =>
                {
                    return await downloader.GetImageAsync(pick_download.DownloadLink, null, d =>
                    {
                        (long cur, long total) = d;
                        notify.Description = $"({cur * 1.0 / total * 100:F2}%) {notify_content}";
                    });
                });
            } while (image == null);

            CurrentDisplayImageLink = pick_download;

            var source = image.ConvertToBitmapImage();

            DetailImageBox.ImageSource = source;
            RefreshButton.IsBusy = false;
        }

        internal class DownloadableImageLinkComparer : IComparer<DownloadableImageLink>
        {
            public static DownloadableImageLinkComparer Instance { get; } = new DownloadableImageLinkComparer();

            public int Compare(DownloadableImageLink x, DownloadableImageLink y)
            {
                //先比较图片尺寸，其次图片文件大小
                var r = (x.Size.Height * x.Size.Width).CompareTo(y.Size.Height * y.Size.Width);
                return r != 0 ? r : x.FileLength.CompareTo(y.FileLength);
            }
        }

        private DownloadableImageLink PickSuitableImageURL(IEnumerable<DownloadableImageLink> downloadableImageLinks)
        {
            var prefer_target = SettingManager.LoadSetting<GlobalSetting>().SelectPreferViewQualityTarget;

            var target_list = downloadableImageLinks.OrderByDescending(x => x, DownloadableImageLinkComparer.Instance).ToArray();

            var result = target_list.Length == 0 ? null : (prefer_target switch
            {
                GlobalSetting.SelectViewQualityTarget.Lowest => target_list.Last(),
                GlobalSetting.SelectViewQualityTarget.Lower => (target_list.Length > 1 ? target_list[target_list.Length - 2] : target_list.First()),
                GlobalSetting.SelectViewQualityTarget.Middle => target_list[target_list.Length / 2],
                GlobalSetting.SelectViewQualityTarget.Higher => (target_list.Length > 1 ? target_list[1] : target_list.First()),
                GlobalSetting.SelectViewQualityTarget.Highest => target_list.First(),
                _ => target_list.Last()
            });

            return result;
        }

        private async Task<long> TryGetVaildDownloadFileSize(string downloadLink)
        {
            try
            {
                return (await RequestHelper.CreateDeafultAsync(downloadLink, req => req.Method = "HEAD")).ContentLength;
            }
            catch (Exception e)
            {
                Log.Warn($"Can't get file size ({downloadLink}) though ContentLength :{e.Message}");
                return 0;
            }
        }

        CancellationTokenSource cancel_source = new CancellationTokenSource();

        public async void ApplyItem(Gallery gallery, GalleryItem item)
        {
            using var _ = LoadingStatus.BeginBusy("正在读取图片详细信息....");

            PictureInfo = item;

            Gallery = gallery ?? Container.Default.GetExportedValues<Gallery>().FirstOrDefault(x => x.GalleryName == item.GalleryName);

            Log<PictureDetailViewPage>.Info($"Apply {gallery}/{item}");

            VoteButton.IsBusy = RefreshButton.IsBusy = MarkButton.IsBusy = true;

            using (var transaction = DB.Database.BeginTransaction())
            {
                var visit_entity = DB.VisitRecords.FirstOrDefault(x => x.GalleryItem.GalleryItemID == item.GalleryItemID && x.GalleryItem.GalleryName == gallery.GalleryName);
                if (visit_entity == null)
                {
                    var visit = new VisitRecord()
                    {
                        GalleryItem = item.ConvertToStorableModel(),
                        LastVisitTime = DateTime.Now
                    };

                    DB.VisitRecords.Add(visit);
                }
                else
                {
                    visit_entity.LastVisitTime = DateTime.Now;
                    DB.Entry(visit_entity).CurrentValues.SetValues(visit_entity);
                }

                await DB.SaveChangesAsync();
                transaction.Commit();
            }

            var is_mark = DB.ItemMarks.Where(x => x.Item.GalleryName == gallery.GalleryName && x.Item.GalleryItemID == item.GalleryItemID).Any();
            var detail = gallery.GetImageDetial(item);

            if (SettingManager.LoadSetting<GlobalSetting>().TryGetVaildDownloadFileSize)
                if (SettingManager.LoadSetting<GlobalSetting>().TryGetVaildDownloadFileSize)
                    foreach (var i in detail.DownloadableImageLinks.Where(x => x.FileLength == 0))
                        i.FileLength = await TryGetVaildDownloadFileSize(i.DownloadLink);

            bool? is_vote = default;

            try
            {
                is_vote = gallery.Feature<IGalleryVote>()?.IsVoted(item);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }

            IsMark = is_mark;
            IsVoted = is_vote ?? false;
            VoteButton.IsBusy = RefreshButton.IsBusy = MarkButton.IsBusy = false;
            PictureDetailInfo = detail;
        }

        private void BrowserOpenButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(((FrameworkElement)sender).DataContext is DownloadableImageLink link))
                return;

            Process.Start(link.DownloadLink);
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigationPop();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void MarkButton_Click(object sender, RoutedEventArgs e)
        {
            if (PictureInfo == null || Gallery == null)
                return;

            MarkButton.IsBusy = true;

            if (!IsMark)
            {
                DB.ItemMarks.Add(new GalleryItemMark()
                {
                    Item = PictureInfo.ConvertToStorableModel(),
                    Time = DateTime.Now
                });

                await DB.SaveChangesAsync();
                IsMark = true;
            }
            else
            {
                var x = DB.ItemMarks.FirstOrDefault(x => x.Item.GalleryName == Gallery.GalleryName && x.Item.GalleryItemID == PictureInfo.GalleryItemID);
                DB.ItemMarks.Remove(x);
                IsMark = false;
            }

            MarkButton.IsBusy = false;

            Log<PictureDetailViewPage>.Debug($"Now IsMark={IsMark}");
        }

        private async void VoteButton_Click(object sender, RoutedEventArgs ee)
        {
            if (PictureInfo == null || Gallery == null)
                return;

            var item = PictureInfo;
            var is_vote = IsVoted;
            var gallery = Gallery;

            VoteButton.IsBusy = true;

            try
            {
                await Task.Run(() => gallery.Feature<IGalleryVote>().SetVote(item, !is_vote));

                if (item == PictureInfo)
                {
                    IsVoted = !is_vote;
                    Toast.ShowMessage($"已{(!is_vote ? "投票" : "取消投票")}");
                }
            }
            catch (Exception e)
            {
                Toast.ShowMessage($"投票失败,{e.Message}");
            }
            finally
            {
                VoteButton.IsBusy = false;
            }
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            var download_link = (sender as FrameworkElement).DataContext as DownloadableImageLink;

            var file_name = string.IsNullOrWhiteSpace(download_link.FullFileName) ?
                $"{FileNameHelper.GetFileNameWithoutExtName(PictureDetailInfo)}{System.IO.Path.GetExtension(download_link.DownloadLink)}"
                : FileNameHelper.FilterFileName(download_link.FullFileName);

            var config = SettingManager.LoadSetting<GlobalSetting>();

            var full_file_path = System.IO.Path.Combine(config.DownloadPath, config.SeparateGallerySubDirectories ? Gallery.GalleryName : "", file_name);

            foreach (var ic in System.IO.Path.GetInvalidPathChars())
                full_file_path = full_file_path.Replace(ic, '_');

            var download_task = new DownloadWrapper()
            {
                DownloadInfo = new Download()
                {
                    DownloadUrl = download_link.DownloadLink,
                    TotalBytes = download_link.FileLength,
                    GalleryItem = PictureInfo.ConvertToStorableModel(),
                    FileName = file_name,
                    DownloadFullPath = full_file_path,
                    DisplayDownloadedLength = 0,
                    DownloadStartTime = DateTime.Now
                }
            };

            if (DownloadManager.CheckIfContained(download_task))
            {
                //jump to download page if download task is exist.

                Toast.ShowMessage("已存在相同的下载任务");

                NavigationHelper.NavigationPush(new DownloadListPage());

                return;
            }

            DownloadManager.DownloadStart(download_task);
            Toast.ShowMessage("开始下载图片...");
        }

        private void AddTagCollectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as FrameworkElement).DataContext is string tag_name))
                return;

            if (TagManager.Contain(tag_name, TagRecordType.Marked))
            {
                Toast.ShowMessage($"已收藏此标签了");
                return;
            }

            TagManager.AddTag(tag_name, Gallery.GalleryName, TagType.Unknown, TagRecordType.Marked);

            Toast.ShowMessage($"添加成功");
        }

        private void AddTagFilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as FrameworkElement).DataContext is string tag_name))
                return;

            if (TagManager.Contain(tag_name, TagRecordType.Filter))
            {
                Toast.ShowMessage($"已过滤此标签了");
                return;
            }

            TagManager.AddTag(tag_name, Gallery.GalleryName, TagType.Unknown, TagRecordType.Filter);

            Toast.ShowMessage($"过滤标签添加成功");
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as FrameworkElement).DataContext is string tag_name))
                return;

            NavigationHelper.NavigationPush(new MainGalleryPage(new[] { tag_name }));
        }

        enum LayoutState
        {
            One, Two, Three
        }

        LayoutState current_layout = LayoutState.One;
        private Storyboard layout_translate_storyboard;

        private void MenuButton_Click_1(object sender, RoutedEventArgs e)
        {
            current_layout = LayoutState.Two;
            ApplyTranslate();
        }

        private void MenuButton_Click_2(object sender, RoutedEventArgs e)
        {
            current_layout = LayoutState.One;
            ApplyTranslate();
        }

        private void MenuButton_Click_3(object sender, RoutedEventArgs e)
        {
            current_layout = LayoutState.Three;
            ApplyTranslate();
        }

        private void MenuButton_Click_4(object sender, RoutedEventArgs e)
        {
            current_layout = LayoutState.Two;
            ApplyTranslate();
        }

        private Thickness CalculateMargin()
        {
            double margin_left = 0;

            switch (current_layout)
            {
                case LayoutState.One:
                    margin_left = 0;
                    break;
                case LayoutState.Two:
                    margin_left = 1;
                    break;
                case LayoutState.Three:
                    margin_left = 2;
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
            layout_translate_storyboard.Begin(MainGrid);
        }

        private void ViewPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var new_margin = CalculateMargin();
            MainGrid.Margin = new_margin;
        }

        public void OnNavigationForwardAction()
        {
            switch (current_layout)
            {
                case LayoutState.One:
                    MenuButton_Click_1(null, null);
                    break;
                case LayoutState.Two:
                    MenuButton_Click_3(null, null);
                    break;
                default:
                    break;
            }
        }

        public void OnNavigationBackAction()
        {
            switch (current_layout)
            {
                case LayoutState.One:
                    MenuButton_Click(null, null);
                    break;
                case LayoutState.Two:
                    MenuButton_Click_2(null, null);
                    break;
                case LayoutState.Three:
                    MenuButton_Click_1(null, null);
                    break;
                default:
                    break;
            }
        }

        private void MenuButton_Click_5(object sender, RoutedEventArgs e)
        {
            ApplyItem(Gallery, PictureInfo);
            ChangeDetailPicture(PictureDetailInfo);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs eee)
        {
            try
            {
                if (DetailImageBox.ImageSource is BitmapSource source)
                {
                    Clipboard.SetImage(source);

                    Toast.ShowMessage("复制成功");
                }
                else
                    Toast.ShowMessage("复制失败,图片未加载");
            }
            catch (Exception e)
            {
                ExceptionHelper.DebugThrow(e);
                Toast.ShowMessage("复制失败," + e.Message);
            }
        }

        private void ViewPage_SizeChanged_1(object sender, SizeChangedEventArgs e)
        {

        }

        private async void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            if (RefreshButton.IsBusy)
            {
                Toast.ShowMessage("请等图片加载完成");
                return;
            }

            var list = PictureDetailInfo.DownloadableImageLinks.OrderBy(x => x, DownloadableImageLinkComparer.Instance).Select((item, index) => (index, item));

            var content = new SelectableImageList(list, CurrentDisplayImageLink);
            await Dialog.ShowDialog(content);

            if (content.CurrentDisplayImageLink == CurrentDisplayImageLink || content.CurrentDisplayImageLink is null)
                return;

            await DisplayImage(content.CurrentDisplayImageLink);
        }
    }
}
