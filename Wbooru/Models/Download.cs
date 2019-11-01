using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Models
{
    public class Download
    {
        public ulong TotalBytes { get; set; }
        public string FileName { get; set; }
        public DateTime DownloadStartTime { get; set; }
        public int GalleryPictureID { get; set; }
        public string GalleryName { get; set; }

        public int DownloadId { get; set; }
    }
}
