using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Models.Gallery;
using Wbooru.PluginExt;

namespace Wbooru.Galleries
{
    public abstract class Gallery : IPlugin
    {
        public abstract string GalleryName { get; }
        public abstract string PluginName { get; }
        public abstract string PluginAuthor { get; }

        public abstract IEnumerable<GalleryItem> GetMainPostedImages();

        public abstract GalleryImageDetail GetImageDetial(string id);

        public abstract IEnumerable<GalleryItem> SearchImages(IEnumerable<string> keywords);
    }
}
