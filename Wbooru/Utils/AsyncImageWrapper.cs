using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Wbooru.Network;
using Wbooru.Utils.Resource;

namespace Wbooru.Utils
{
    public class AsyncImageWrapper : DependencyObject
    {
        public AsyncImageWrapper(string name,string dl) :this(async () =>
        { 
            var downloader = Container.Default.GetExportedValue<ImageFetchDownloadSchedule>();
            var resource = Container.Default.GetExportedValue<ImageResourceManager>();

            var image = await resource.RequestImageAsync(name, () =>
            {
                return downloader.GetImageAsync(dl).Result;
            });

            using var stream = new MemoryStream();
            image.Save(stream, image.RawFormat);
            stream.Seek(0, SeekOrigin.Begin);

            BitmapImage source = null;
            source = new BitmapImage();
            source.BeginInit();
            source.StreamSource = stream;
            source.CacheOption = BitmapCacheOption.OnLoad;
            source.EndInit();
            source.Freeze();

            return source;
        })
        {

        }

        public AsyncImageWrapper(Func<Task<BitmapImage>> valueFunc)
        {
            LoadValue(valueFunc);
        }

        private async void LoadValue(Func<Task<BitmapImage>> valueFunc)
        {
            AsyncValue = await Task.Run(() =>
            {
                return valueFunc();
            });
        }

        public BitmapImage AsyncValue
        {
            get { return (BitmapImage)GetValue(AsyncValueProperty); }
            set { SetValue(AsyncValueProperty, value); }
        }

        public static readonly DependencyProperty AsyncValueProperty =
            DependencyProperty.Register("AsyncValue", typeof(BitmapImage), typeof(AsyncImageWrapper), new PropertyMetadata(default(BitmapImage)));
    }
}
