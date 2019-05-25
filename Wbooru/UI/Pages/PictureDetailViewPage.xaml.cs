using System;
using System.Collections.Generic;
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

        // Using a DependencyProperty as the backing store for PictureDetailInfo.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PictureDetailInfoProperty =
            DependencyProperty.Register("PictureDetailInfo", typeof(GalleryImageDetail), typeof(PictureDetailViewPage), new PropertyMetadata(null));

        public PictureDetailViewPage()
        {
            InitializeComponent();

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

            var current=Task.Run(() => {
                var detail=gallery.GetImageDetial(item);
                Dispatcher.Invoke(() => PictureDetailInfo = detail);
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
    }
}
