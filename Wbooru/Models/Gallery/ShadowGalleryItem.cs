using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Models.Gallery
{
    public class ShadowGalleryItem:GalleryItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public GalleryItem ConvertToNormalModel()
        {
            return new GalleryItem()
            {
                PreviewImageDownloadLink = this.PreviewImageDownloadLink,
                DownloadFileName = this.DownloadFileName,
                PreviewImageSize = this.PreviewImageSize,
                GalleryItemID = this.GalleryItemID,
            };
        }
    }
}
