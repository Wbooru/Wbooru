using System;
using System.Collections.Generic;
using System.Drawing;
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
    }
}
