using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using Wbooru.Models;

namespace Wbooru.UI.ValueConverters
{
    public class TagColorConverter : IValueConverter
    {
        private static readonly Dictionary<TagType, Brush> cached_brush = new Dictionary<TagType, Brush>()
        {
            {TagType.Artist,new SolidColorBrush(Color.FromRgb(204,204,0))},
            {TagType.Character,new SolidColorBrush(Color.FromRgb(0,170,0))},
            {TagType.Copyright,new SolidColorBrush(Color.FromRgb(221,0,221))},
            {TagType.General,new SolidColorBrush(Color.FromRgb(238,136,135))},
            {TagType.Circle,new SolidColorBrush(Color.FromRgb(0,187,187))},
            {TagType.Faults,new SolidColorBrush(Color.FromRgb(255,32,32))},
            {TagType.Unknown,new SolidColorBrush(Colors.White)},
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is TagType tag_type))
                return cached_brush[TagType.Unknown];

            return cached_brush[tag_type];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
