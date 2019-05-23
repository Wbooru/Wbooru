using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Wbooru.ValueConverters
{
    public class CustomBindableMarginConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var r = new double[4] {0,0,0,0};

            for (int i = 0; i < values.Length; i++)
            {
                if (!double.TryParse(values[i]?.ToString() ?? "0", out var v))
                    v = 0;

                r[i] = v;
            }

            return new Thickness(r[0], r[1], r[2], r[3]);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
