using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Galleries;
using Wbooru.Models.Gallery;

namespace YandeSourcePlugin
{
    public class PictureItem : GalleryItem
    {
        public GalleryImageDetail GalleryDetail { get; set; }
    }
}
