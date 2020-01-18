using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Models.Gallery;

namespace Wbooru.Galleries.SupportFeatures
{
    public interface IGalleryMark : IGalleryFeature
    {
        Task SetMarkAsync(GalleryItem item,bool is_mark);
        Task<bool> IsMarkedAsync(GalleryItem item);
        IAsyncEnumerable<GalleryItem> GetMarkedGalleryItemAsync();
    }
}
