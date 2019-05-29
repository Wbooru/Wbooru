using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Persistence
{
    public class DownloadRecord
    {
        public int DownloadRecordID { get; set; }

        public string GalleryID { get; set; }
        public string GalleryName { get; set; }
        public string DownloadFileName { get; set; }
        public DateTime DownloadTime { get; set; }
    }
}
