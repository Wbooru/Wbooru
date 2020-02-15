using System;
using System.Collections.Generic;
using System.Drawing;
using Wbooru.Models.Gallery.Annotation;

namespace Wbooru.Models.Gallery
{
    public class GalleryImageDetail
    {
        [DisplayName("图片ID")]
        [DisplayOrder(-10)]
        public string ID { get; set; }

        [DisplayName("创建日期")]
        public DateTime CreateDate { get; set; }

        [DisplayName("作者")]
        public string Author { get; set; }

        [DisplayName("上传者")]
        public string Updater { get; set; }

        [DisplayName("图片分辨率")]
        public Size Resolution { get; set; }

        [DisplayName("出处")]
        public string Source { get; set; }

        [DisplayName("评级")]
        public string Rate { get; set; }

        [DisplayName("分数")]
        public string Score { get; set; }

        public IEnumerable<string> Tags { get; set; }

        public IEnumerable<DownloadableImageLink> DownloadableImageLinks { get; set; }
    }
}
