using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Wbooru.Models.SettingUI;
using Wbooru.Settings.UIAttributes;
using Wbooru.Utils;

namespace Wbooru.UI.ValueConverters.SettingUI
{
    public class RangeValueConverter<T> : IValueConverter where T : IComparable
    {
        public static TypeConverter Converter { get; } = TypeDescriptor.GetConverter(typeof(T));

        public RangeValueConverter(RangeAttribute range)
        {
            Min = (T)Converter.ConvertFrom(range.Min);
            Max = (T)Converter.ConvertFrom(range.Max);
        }

        public T Min { get; }
        public T Max { get; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(Converter.ConvertFrom(value.ToString()) is T val))
            {
                return (parameter as PropertyInfoWrapper).ProxyValue;
            }

            return MathEx.Max(Min, MathEx.Min(Max, val));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
