using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Models.Gallery;

namespace Wbooru.Galleries.SupportFeatures
{
    public interface IGallerySearchImage : IGalleryFeature
    {
        IAsyncEnumerable<GalleryItem> SearchImagesAsync(IEnumerable<string> keywords);
    }
}
