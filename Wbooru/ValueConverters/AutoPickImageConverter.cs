using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Wbooru.Models.Gallery;
using Wbooru.Utils;

namespace Wbooru.ValueConverters
{
    public class AutoPickImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((!(value is GalleryImageDetail detail)) || (!detail.DownloadableImageLinks.Any()))
                return null;

            var pick_download = detail.DownloadableImageLinks.OrderByDescending(x => x.FileLength).FirstOrDefault();

            return new AsyncImageWrapper(pick_download.DownloadLink, pick_download.DownloadLink);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
