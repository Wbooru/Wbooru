using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Wbooru.Utils
{
    public static class ImageExtension
    {
        public static BitmapImage ConvertToBitmapImage(this Image image)
        {
            using var stream = new MemoryStream();
            image.Save(stream, image.RawFormat);
            stream.Seek(0, SeekOrigin.Begin);

            var source = new BitmapImage();
            source.BeginInit();
            source.StreamSource = stream;
            source.CacheOption = BitmapCacheOption.OnLoad;
            source.EndInit();
            source.Freeze();

            return source;
        }
    }
}
