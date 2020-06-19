using Microsoft.EntityFrameworkCore;
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
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public ImageSize PreviewImageSize { get; set; }
        public string PreviewImageDownloadLink { get; set; }
        public string DownloadFileName { get; set; }    
        public string GalleryName { get; set; }

        public string GalleryItemID { get; set; }

        public override string ToString() => $"{GalleryItemID} {DownloadFileName}";
    }
}
