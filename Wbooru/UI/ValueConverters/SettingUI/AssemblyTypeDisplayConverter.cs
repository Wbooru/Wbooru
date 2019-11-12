using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Wbooru.Settings;

namespace Wbooru.UI.ValueConverters.SettingUI
{
    public class AssemblyTypeDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                Assembly a => a.FullName.Split(',').First(),
                Type t => t.Name,
                _ => throw new Exception("Unsupport type")
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
