using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Wbooru.Models.Gallery.Annotation;
using Wbooru.Settings;
using static Wbooru.Settings.GlobalSetting;
using static Wbooru.UI.Pages.PictureDetailViewPage;

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

        public DownloadableImageLink PickSuitableImageURL(SelectViewQualityTarget qualityTarget)
        {
            var target_list = DownloadableImageLinks.OrderByDescending(x => x, DownloadableImageLinkComparer.Instance).ToArray();

            var result = target_list.Length == 0 ? null : (qualityTarget switch
            {
                SelectViewQualityTarget.Lowest => target_list.Last(),
                SelectViewQualityTarget.Lower => (target_list.Length > 1 ? target_list[target_list.Length - 2] : target_list.First()),
                SelectViewQualityTarget.Middle => target_list[target_list.Length / 2],
                SelectViewQualityTarget.Higher => (target_list.Length > 1 ? target_list[1] : target_list.First()),
                SelectViewQualityTarget.Highest => target_list.First(),
                _ => target_list.Last()
            });

            return result;
        }
    }
}
