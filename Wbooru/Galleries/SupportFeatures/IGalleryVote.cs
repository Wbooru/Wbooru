using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Models.Gallery;

namespace Wbooru.Galleries.SupportFeatures
{
    public interface IGalleryVote : IGalleryFeature
    {
        Task SetVoteAsync(GalleryItem item, bool is_mark);
        Task<bool> IsVotedAsync(GalleryItem item);
        IAsyncEnumerable<GalleryItem> GetVotedGalleryItemsAsync();
    }
}
