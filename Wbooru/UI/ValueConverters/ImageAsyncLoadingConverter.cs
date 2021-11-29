using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Wbooru.Models.Gallery;
using Wbooru.Network;
using Wbooru.PluginExt;
using Wbooru.Utils;
using Wbooru.Utils.Resource;

namespace Wbooru.UI.ValueConverters
{
    public class ImageAsyncLoadingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GalleryItem item)
            {
                return new AsyncImageWrapper(item.DownloadFileName, item.PreviewImageDownloadLink);
            }
            else if (value is ImageAsyncLoadingParam param)
            {
                return new AsyncImageWrapper(param.ImageUrl, param.PreviewImageDownloadUrl);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
