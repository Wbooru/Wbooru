using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Wbooru.Galleries;

namespace Wbooru.UI.ValueConverters
{
    public class GalleryFeatureContainConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Gallery gallery))
                return false;

            if (!Enum.TryParse<GalleryFeature>(parameter?.ToString(), out var e))
                return false;

            return gallery.SupportFeatures.HasFlag(e);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
