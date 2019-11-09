using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wbooru.Models.Gallery;
using Wbooru.Utils;

namespace Wbooru.Models
{
    public class Download
    {
        public Download()
        {
            DownloadId = MathEx.Random(max: -1);
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DownloadId { get; set; }

        /// <summary>
        /// 文件大小
        /// </summary>
        public long TotalBytes { get; set; }

        /// <summary>
        /// 开始下载时间
        /// </summary>
        public DateTime DownloadStartTime { get; set; } = DateTime.Now;

        public string DownloadUrl { get; set; }

        public virtual ShadowGalleryItem GalleryItem { get; set; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 完整图片文件路径
        /// </summary>
        public string DownloadFullPath { get; set; }

        public long DisplayDownloadedLength { get; set; }

        public bool CheckIfSame(Download another)
        {
            return another.GalleryItem.GalleryName == GalleryItem.GalleryName
                && another.GalleryItem.GalleryItemID == GalleryItem.GalleryItemID
                && another.FileName == FileName 
                && another.DownloadUrl == DownloadUrl;
        }
    }
}
