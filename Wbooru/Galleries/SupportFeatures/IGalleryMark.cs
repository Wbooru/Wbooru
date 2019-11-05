using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Models.Gallery;

namespace Wbooru.Galleries.SupportFeatures
{
    public interface IGalleryMark
    {
        void SetMark(GalleryItem item,bool is_mark);
        bool IsMarked(GalleryItem item);
        IEnumerable<GalleryItem> GetMarkedGalleryItem();
    }
}
