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
            return new AsyncTask(() => {
                var downloader = Container.Default.GetExportedValue<ImageFetchDownloadSchedule>();
                var image = downloader.GetImageAsync(value.ToString()).Result;

                using var stream = new MemoryStream();
                image.Save(stream, image.RawFormat);
                stream.Seek(0, SeekOrigin.Begin);

                BitmapImage source=null;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    source=new BitmapImage();
                    source.BeginInit();
                    source.StreamSource = stream;
                    source.CacheOption = BitmapCacheOption.OnLoad;
                    source.EndInit();
                    source.Freeze();
                });

                return source;
            });
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public class AsyncTask : INotifyPropertyChanged
        {
            public AsyncTask(Func<BitmapImage> valueFunc)
            {
                AsyncValue = default(BitmapImage);
                LoadValue(valueFunc);
            }

            private async void LoadValue(Func<BitmapImage> valueFunc)
            {
                AsyncValue = await Task.Run(() =>
                {
                    return valueFunc();
                });

                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("AsyncValue"));
            }

            public BitmapImage AsyncValue { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;
        }
    }
}
