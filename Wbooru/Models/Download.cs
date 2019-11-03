using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Wbooru.Models
{
    public class Download
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DownloadId { get; set; } = -10000;

        /// <summary>
        /// 文件大小
        /// </summary>
        public ulong TotalBytes { get; set; }

        /// <summary>
        /// 开始下载时间
        /// </summary>
        public DateTime DownloadStartTime { get; set; } = DateTime.Now;

        public int GalleryPictureID { get; set; }

        public string DownloadUrl { get; set; }

        public string GalleryName { get; set; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 完整图片文件路径
        /// </summary>
        public string DownloadFullPath { get; set; }

        public long DisplayDownloadedLength { get; set; }
    }
}
