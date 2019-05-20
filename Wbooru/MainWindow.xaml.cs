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

namespace Wbooru
{
    using Logger = Log<GalleryWindow>;

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class GalleryWindow : Window
    {
        public GalleryItemUIElementWrapper ItemCollectionWrapper { get; private set; } = new GalleryItemUIElementWrapper();

        public Gallery CurrentGallery
        {
            get { return (Gallery)GetValue(CurrentGalleryProperty); }
            set { SetValue(CurrentGalleryProperty, value); }
        }

        public static readonly DependencyProperty CurrentGalleryProperty =
            DependencyProperty.Register("gallery", typeof(Gallery), typeof(GalleryWindow), new PropertyMetadata(null));

        public GlobalSetting Setting { get; private set; }
        public ImageResourceManager Resource { get; private set; }
        public ImageFetchDownloadSchedule ImageDownloader { get; private set; }

        public GalleryWindow()
        {
            InitializeComponent();

            DataContext = this;

            try
            {
                var gallery = Container.Default.GetExportedValue<Gallery>();
                Setting = Container.Default.GetExportedValue<SettingManager>().LoadSetting<GlobalSetting>();
                Resource = Container.Default.GetExportedValue<ImageResourceManager>();
                ImageDownloader = Container.Default.GetExportedValue<ImageFetchDownloadSchedule>();

                if (gallery != null)
                    ApplyGallery(gallery);
            }
            catch (Exception e)
            {
                Logger.Warn("Failed to get a gallery.:" + e.Message);
            }
        }

        public void ApplyGallery(Gallery gallery)
        {
            ItemCollectionWrapper.Pictures.Clear();

            CurrentGallery = gallery;

            var task=Task.Run(() => {
                var list = gallery.GetMainPostedImages().Take(Setting.GetPictureCountPerLoad).ToArray();

                Dispatcher.Invoke(() => {
                    foreach (var item in list)
                        ItemCollectionWrapper.Pictures.Add(item);
                });
            });
        }

        #region Left Menu Show/Hide

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
                    var hide_sb = Resources["HideLeftPane"] as Storyboard;
                    hide_sb.Begin(MainGrid);
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
            Close();
        }
    }
}
