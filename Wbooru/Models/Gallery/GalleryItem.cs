using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Models.Gallery
{
    public abstract class GalleryItem
    {
        public string PreviewImageDownloadLink { get; set; }
        public string DownloadFileName { get; set; }
        public Size PreviewImageSize { get; set; }
        public string ID { get; set; }

        public override string ToString() => $"{ID} {DownloadFileName}";
    }
}
