using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Wbooru.Models.Gallery;

namespace Wbooru.ValueConverters
{
    public class FormatDownloadDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is DownloadableImageLink download_info))
                return "Undownloadable";

            return $"{download_info.Description} ({download_info.Size.Width}*{download_info.Size.Height}) {FormatFileSize(download_info.FileLength)}";
        }

        private static string FormatFileSize(long bytes)
        {
            if (bytes <= 0) return "<unknown file size>";
            var units = new []{ "B", "kB", "MB", "GB", "TB" };
            int digitGroups = (int)(Math.Log10(bytes) / Math.Log10(1024));
            return $"{bytes / Math.Pow(1024, digitGroups):F2}  {units[digitGroups]}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
