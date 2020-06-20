using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Models.Gallery;

namespace Wbooru.Models
{
    public class GalleryItemMark
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GalleryItemMarkID { get; set; }

        public DateTime Time { get; set; }

        public virtual GalleryItem GalleryItem { get; set; }
    }
}
