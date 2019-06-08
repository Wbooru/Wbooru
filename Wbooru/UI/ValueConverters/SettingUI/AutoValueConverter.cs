using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Wbooru.Models.SettingUI;

namespace Wbooru.UI.ValueConverters.SettingUI
{
    public class AutoValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return TypeDescriptor.GetConverter(targetType).ConvertFrom(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var wrapper=parameter as PropertyInfoWrapper;
            var converter=TypeDescriptor.GetConverter(wrapper.PropertyInfo.PropertyType);

            var str = value.ToString();

            return converter.ConvertFrom(string.IsNullOrWhiteSpace(str) ? wrapper.ProxyValue.ToString() : str);
        }
    }
}
