using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Wbooru.Network;
using Wbooru.PluginExt;

namespace Wbooru.ValueConverters
{
    public class ImageAsyncLoadingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
             var downloader = Container.Default.GetExportedValue<ImageFetchDownloadSchedule>();
            var image=downloader.GetImageAsync(value.ToString()).Result;

            using var stream = new MemoryStream();
            image.Save(stream,image.RawFormat);
            stream.Seek(0, SeekOrigin.Begin);

            BitmapImage source = new BitmapImage();
            source.BeginInit();
            source.StreamSource = stream;
            source.CacheOption = BitmapCacheOption.OnLoad;
            source.EndInit();
            source.Freeze();

            //todo
            return source;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
