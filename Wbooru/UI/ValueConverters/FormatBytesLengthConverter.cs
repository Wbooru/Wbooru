using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Wbooru.UI.ValueConverters
{
    public class FormatBytesLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return FormatFileSize((long)value);
        }

        private static string FormatFileSize(long bytes)
        {
            if (bytes <= 0) return "0B";
            var units = new[] { "B", "kB", "MB", "GB", "TB" };
            int digitGroups = (int)(Math.Log10(bytes) / Math.Log10(1024));
            return $"{bytes / Math.Pow(1024, digitGroups):F2}  {units[digitGroups]}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
