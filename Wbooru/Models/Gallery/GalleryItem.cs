using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Models.Gallery
{
    public class GalleryItem
    {
        public string PreviewImageDownloadLink { get; set; }
        public string DownloadFileName { get; set; }
        public Size PreviewImageSize { get; set; }
        public string GalleryName { get; set; }
        public string GalleryItemID { get; set; }

        public override string ToString() => $"{GalleryItemID} {DownloadFileName}";

        public ShadowGalleryItem ConvertToStorableModel()
        {
            return new ShadowGalleryItem()
            {
                PreviewImageDownloadLink = this.PreviewImageDownloadLink,
                DownloadFileName = this.DownloadFileName,
                PreviewImageSize = this.PreviewImageSize,
                GalleryName = this.GalleryName,
                GalleryItemID = this.GalleryItemID,
            };
        }
    }
}
