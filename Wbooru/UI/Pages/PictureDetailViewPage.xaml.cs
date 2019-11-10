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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wbooru.Galleries;
using Wbooru.Kernel;
using Wbooru.Models;
using Wbooru.Models.Gallery;
using Wbooru.Network;
using Wbooru.Persistence;
using Wbooru.Settings;
using Wbooru.UI.Controls;
using Wbooru.Utils;
using Wbooru.Utils.Resource;
using Brushes = System.Windows.Media.Brushes;

namespace Wbooru.UI.Pages
{
    /// <summary>
    /// PictureDetailViewPage.xaml 的交互逻辑
    /// </summary>
    public partial class PictureDetailViewPage : Page,ICacheCleanable
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
            DependencyProperty.Register("IsMark", typeof(bool), typeof(PictureDetailViewPage), new PropertyMetadata(false));

        public bool IsVoted
        {
            get { return (bool)GetValue(IsVotedProperty); }
            set { SetValue(IsVotedProperty, value); }
        }

        public static readonly DependencyProperty IsVotedProperty =
            DependencyProperty.Register("IsVoted", typeof(bool), typeof(PictureDetailViewPage), new PropertyMetadata(false));

        public LocalDBContext DB { get; }

        public PictureDetailViewPage()
        {
            InitializeComponent();

            DB = LocalDBContext.Instance;

            MainGrid.DataContext = this;
        }

        private void ChangeDetailPicture(GalleryImageDetail galleryImageDetail)
        {
            if (galleryImageDetail == null)
            {
                //clean.
                DetailImageBox.ImageSource = null;
                return;
            }

            Task.Run(() =>
            {
                using (var _ = LoadingStatus.BeginBusy("加载图片中......"))
                {
                    var pick_download = galleryImageDetail.DownloadableImageLinks.OrderByDescending(x => x.FileLength).FirstOrDefault();

                    if (pick_download == null)
                    {
                        //notice error;
                        return;
                    }

                    var downloader = Container.Default.GetExportedValue<ImageFetchDownloadScheduler>();

                    System.Drawing.Image image;

                    do
                    {
                        image = ImageResourceManager.RequestImageAsync(pick_download.FullFileName, () =>
                        {
                            return downloader.GetImageAsync(pick_download.DownloadLink).Result;
                        }).Result;
                    } while (image == null);

                    var source = image.ConvertToBitmapImage();

                    Dispatcher.Invoke(() =>
                    {
                        if (PictureDetailInfo == galleryImageDetail)
                        {
                            DetailImageBox.ImageSource = source;
                        }
                        else
                        {
                            Log<PictureDetailViewPage>.Debug($"Picture info mismatch.");
                        }
                    });
                }
            }, cancel_source.Token);
        }

        CancellationTokenSource cancel_source = new CancellationTokenSource();

        public void ApplyItem(Gallery gallery, GalleryItem item)
        {
            var notify = LoadingStatus.BeginBusy("正在读取图片详细信息....");

            Gallery = gallery??Container.Default.GetExportedValues<Gallery>().FirstOrDefault(x=>x.GalleryName==item.GalleryName);
            PictureInfo = item;

            Log<PictureDetailViewPage>.Info($"Apply {gallery}/{item}");
            MarkButton.IsEnabled = false;

            Task.Run(() =>
            {
                using (var transaction = DB.Database.BeginTransaction())
                {
                    var visit = new VisitRecord()
                    {
                        GalleryItem = item.ConvertToStorableModel(),
                        LastVisitTime = DateTime.Now
                    };

                    var visit_entity = DB.VisitRecords.FirstOrDefault(x => x.GalleryItem.GalleryItemID == item.GalleryItemID && x.GalleryItem.GalleryName == gallery.GalleryName);
                    if (visit_entity == null)
                        DB.VisitRecords.Add(visit);
                    else
                        DB.Entry(visit_entity).CurrentValues.SetValues(visit_entity);

                    DB.SaveChanges();
                    transaction.Commit();
                }

                var is_mark = DB.ItemMarks.Where(x => x.Item.GalleryName == gallery.GalleryName && x.Item.GalleryItemID == item.GalleryItemID).Any();
                var detail = gallery.GetImageDetial(item);

                Dispatcher.Invoke(() =>
                {
                    IsMark = is_mark;
                    MarkButton.IsEnabled = true;
                    PictureDetailInfo = detail;
                    notify.Dispose();
                });
            }, cancel_source.Token);
        }

        public void OnBeforeGetClean()
        {

        }

        public void OnAfterPutClean()
        {
            Gallery = null;
            PictureInfo = null;
            PictureDetailInfo = null;
            IsMark = false;
            IsVoted = false;

            try
            {
                cancel_source.Cancel();
                cancel_source = new CancellationTokenSource();
            }
            catch { }

            LoadingStatus.ForceFinishAllStatus();
        }

        private void BrowserOpenButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(((FrameworkElement)sender).DataContext is DownloadableImageLink link))
                return;

            Process.Start(link.DownloadLink);
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            var page= NavigationHelper.NavigationPop() as PictureDetailViewPage;

            ObjectPool<PictureDetailViewPage>.Return(page);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MarkButton_Click(object sender, RoutedEventArgs e)
        {
            if (PictureInfo == null || Gallery==null)
                return;


            if (!IsMark)
            {
                DB.ItemMarks.Add(new GalleryItemMark()
                {
                    Item= PictureInfo.ConvertToStorableModel(),
                    Time = DateTime.Now
                });

                DB.SaveChanges();
                IsMark = true;
            }
            else
            {
                var x = DB.ItemMarks.FirstOrDefault(x => x.Item.GalleryName == Gallery.GalleryName && x.Item.GalleryItemID == PictureInfo.GalleryItemID);
                DB.ItemMarks.Remove(x);
                IsMark = false;
            }

            Log<PictureDetailViewPage>.Debug($"Now IsMark={IsMark}");
        }

        private void VoteButton_Click(object sender, RoutedEventArgs e)
        {
            if (PictureInfo == null || Gallery == null)
                return;

            //todo
            IsVoted = !IsVoted;

            Log<PictureDetailViewPage>.Debug($"Now IsVoted={IsVoted}");
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            var download_link = (sender as FrameworkElement).DataContext as DownloadableImageLink;

            var file_name = string.IsNullOrWhiteSpace(download_link.FullFileName)? 
                string.Join("", $"{PictureDetailInfo.ID} {string.Join("_", PictureDetailInfo.Tags)}{System.IO.Path.GetExtension(download_link.DownloadLink)}")
                : download_link.FullFileName;

            foreach (var ic in System.IO.Path.GetInvalidFileNameChars())
                file_name = file_name.Replace(ic, '_');

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

            if (TagManager.Contain(tag_name, false))
            {
                Toast.ShowMessage($"已收藏此标签了");
                return;
            }

            TagManager.AddTag(tag_name, Gallery.GalleryName, TagType.Unknown, false);

            Toast.ShowMessage($"添加成功");
        }

        private void AddTagFilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as FrameworkElement).DataContext is string tag_name))
                return;

            if (TagManager.Contain(tag_name,true))
            {
                Toast.ShowMessage($"已过滤此标签了");
                return;
            }

            TagManager.AddTag(tag_name, Gallery.GalleryName, TagType.Unknown, true);

            Toast.ShowMessage($"过滤标签添加成功");
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as FrameworkElement).DataContext is string tag_name))
                return;

            NavigationHelper.NavigationPush(new MainGalleryPage(new[] { tag_name }));
        }
    }
}
