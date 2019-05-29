using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
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
using Wbooru.Models.Gallery;
using Wbooru.Persistence;
using Wbooru.Utils;

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
            DependencyProperty.Register("PictureDetailInfo", typeof(GalleryImageDetail), typeof(PictureDetailViewPage), new PropertyMetadata(null));

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

        [Import(typeof(LocalDBContext))]
        public LocalDBContext DB { get; set; }

        public PictureDetailViewPage()
        {
            InitializeComponent();

            Container.Default.ComposeParts(this);

            MainGrid.DataContext = this;
        }

        CancellationTokenSource source ;

        public void ApplyItem(Gallery gallery,GalleryItem item)
        {
            try
            {
                source.Cancel();
            }
            catch{}

            Gallery = gallery;
            PictureInfo = item;

            Log<PictureDetailViewPage>.Info($"Apply {gallery}/{item}");

            source = new CancellationTokenSource();
            MarkButton.IsEnabled = false;

            var current=Task.Run(() => {
                var visit = new VisitRecord()
                {
                    GalleryID = item.ID,
                    GalleryName = gallery.GalleryName,
                    LastVisitTime = DateTime.Now
                };

                var visit_entity=DB.VisitRecords.FirstOrDefault(x=>x.GalleryID == item.ID && x.GalleryName==gallery.GalleryName);
                if (visit_entity == null)
                    DB.VisitRecords.Add(visit);
                else
                    DB.Entry(visit_entity).CurrentValues.SetValues(visit_entity);

                DB.SaveChanges();

                var x=DB.ItemMarks.Where(x => x.GalleryName == gallery.GalleryName && x.MarkGalleryID == item.ID).Any();
                var detail=gallery.GetImageDetial(item);

                Dispatcher.Invoke(() => {
                    IsVoted = x;
                    MarkButton.IsEnabled = true;
                    PictureDetailInfo = detail; });
            }, source.Token);
        }

        public void OnBeforeGetClean()
        {
            //needn't impl
        }

        public void OnAfterPutClean()
        {
            Gallery = null;
            PictureInfo = null;
            PictureDetailInfo = null;
        }

        private void BrowserOpenButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(((FrameworkElement)sender).DataContext is DownloadableImageLink link))
                return;

            Process.Start(link.DownloadLink);
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            var navigation = Container.Default.GetExportedValue<NavigationHelper>();
            var page=navigation.NavigationPop() as PictureDetailViewPage;

            ObjectPool<PictureDetailViewPage>.Return(page);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MarkButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsMark)
            {
                DB.ItemMarks.Add(new GalleryItemMark()
                {
                    MarkGalleryID = PictureInfo.ID,
                    GalleryName = Gallery.GalleryName,
                    Time = DateTime.Now
                });

                DB.SaveChanges();
                IsMark = true;
            }
            else
            {
                var x = DB.ItemMarks.FirstOrDefault(x => x.GalleryName == Gallery.GalleryName && x.MarkGalleryID == PictureInfo.ID);
                DB.ItemMarks.Remove(x);
                IsMark = false;
            }
        }

        private void VoteButton_Click(object sender, RoutedEventArgs e)
        {
            //todo
            IsVoted = true;
        }
    }
}
