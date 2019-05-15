using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Galleries;
using Wbooru.Models.Gallery;

namespace YandeSourcePlugin
{
    public class PictureItem : GalleryItem, IContainDetail
    {
        public GalleryImageDetail GalleryDetail { get; set; }
    }
}
