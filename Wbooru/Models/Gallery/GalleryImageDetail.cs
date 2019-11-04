using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Models.Gallery
{
    public class GalleryImageDetail
    {
        public DateTime CreateDate { get; set; }
        public string ID { get; set; }
        public string Author { get; set; }
        public string Updater { get; set; }
        public Size Resolution { get; set; }
        public string Source { get; set; }
        public string Rate { get; set; }
        public string Score { get; set; }

        public IEnumerable<string> Tags { get; set; }

        public IEnumerable<DownloadableImageLink> DownloadableImageLinks { get; set; }
    }
}
