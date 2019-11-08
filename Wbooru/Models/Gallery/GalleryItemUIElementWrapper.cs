using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Galleries;

namespace Wbooru.Models.Gallery
{
    public class GalleryItemUIElementWrapper
    {
        public Galleries.Gallery HostGallery { get; set; }
        public ObservableCollection<GalleryItem> Pictures { get; set; } = new ObservableCollection<GalleryItem>();
    }
}
