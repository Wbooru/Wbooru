using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Models.Gallery;

namespace Wbooru.Galleries.SupportFeatures
{
    public interface IGalleryVote
    {
        void SetVote(GalleryItem item, bool is_mark);
        bool IsVoted(GalleryItem item);
        IEnumerable<GalleryItem> GetVotedGalleryItem();
    }
}
