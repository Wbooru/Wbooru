using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Persistence
{
    public class GalleryItemMark
    {
        [Key]
        public int GalleryItemMarkID { get; set; }

        public string GalleryName { get; set; }
        public DateTime Time { get; set; }
        public string MarkGalleryID { get; set; }
    }
}
