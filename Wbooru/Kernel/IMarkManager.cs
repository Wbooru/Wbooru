using System.Collections.Generic;
using System.Threading.Tasks;
using Wbooru.Galleries;
using Wbooru.Kernel.DI;
using Wbooru.Models.Gallery;

namespace Wbooru.Kernel
{
    public interface IMarkManager : IImplementInjectable
    {
        Task SetMark(Gallery gallery, GalleryItem item, bool is_mark);
        Task<bool> GetMark(Gallery gallery, GalleryItem item);
        IAsyncEnumerable<GalleryItem> GetMarkedListAsync(params Gallery[] filterGalleries);
    }
}
