using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Wbooru.Network;
using Wbooru.PluginExt;

namespace Wbooru.ValueConverters
{
    public class ImageAsyncLoadingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new AsyncImageWrapper(async () =>
            {
                var downloader = Container.Default.GetExportedValue<ImageFetchDownloadSchedule>();
                var image = await downloader.GetImageAsync(value.ToString());

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
            });
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public class AsyncImageWrapper : DependencyObject
        {
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
}
