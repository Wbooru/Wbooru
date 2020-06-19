using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Models.Gallery;

namespace Wbooru.Models
{
    public class GalleryItemMark
    {
        [Key]
        public int GalleryItemMarkID { get; set; }

        public DateTime Time { get; set; }

        public virtual GalleryItem Item { get; set; }
    }
}
