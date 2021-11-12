using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Wbooru.Utils;

namespace Wbooru.UI.ValueConverters
{
    public class CustomBindableMarginConverter : IMultiValueConverter
    {
        public CalculatableFormatter Formatter { get; set; } = Container.Get<CalculatableFormatter>();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var format = parameter.ToString().Replace("\\","");

            var b = format;

            for (int i = 0; i < values.Length; i++)
            {
                format = format.Replace($"{{{i}}}", values[i].ToString());
            }

            var c = format;

            format = Formatter.FormatCalculatableString(format);

            var rz = format.Split(',').Select(x => string.IsNullOrWhiteSpace(x)?0:double.Parse(x)).ToArray();
            var r = Enumerable.Range(0, 4).Select(x => x >= rz.Length ? 0 : rz[x]).ToArray();

            Log<CustomBindableMarginConverter>.Debug($"{b}  -->  {c}  ->  {format}");

            return new Thickness(r[0], r[1], r[2], r[3]);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
