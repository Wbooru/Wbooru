using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Wbooru.UI.ValueConverters
{
    public class DisplaySpeedConverter : IValueConverter
    {
        FormatBytesLengthConverter formatter = new FormatBytesLengthConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return $"{formatter.Convert(value, targetType, parameter, culture)}/s".ToLower();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
