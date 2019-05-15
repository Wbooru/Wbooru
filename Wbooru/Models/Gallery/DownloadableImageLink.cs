using System.Drawing;

namespace Wbooru.Models.Gallery
{
    public class DownloadableImageLink
    {
        public string Description { get; set; }
        public Size Size { get; set; }
        public long FileLength { get; set; }
        public string DownloadLink { get; set; }
    }
}