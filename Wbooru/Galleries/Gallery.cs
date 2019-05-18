using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Models.Gallery;
using Wbooru.PluginExt;

namespace Wbooru.Galleries
{
    public abstract class Gallery
    {
        public abstract string GalleryName { get; }

        public abstract IEnumerable<GalleryItem> GetMainPostedImages();

        public abstract GalleryImageDetail GetImageDetial(GalleryItem item);

        public abstract IEnumerable<GalleryItem> SearchImages(IEnumerable<string> keywords);
    }
}
