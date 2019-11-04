using System;
using System.Drawing;

namespace Wbooru.Models.Gallery
{
    public class DownloadableImageLink : IComparable<DownloadableImageLink>
    {
        public string Description { get; set; }
        public Size Size { get; set; }
        public long FileLength { get; set; }
        public string DownloadLink { get; set; }

        /// <summary>
        /// 包含后缀名
        /// </summary>
        public string FullFileName { get; set; }

        public int CompareTo(DownloadableImageLink other)
        {
            return FileLength.CompareTo(other.FileLength);
        }
    }
}