using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Models;

namespace Wbooru.Galleries.SupportFeatures
{
    public interface IGalleryTagDataPredownloadAndCache : IGalleryFeature
    {
        IAsyncEnumerable<(Tag TagInfo,DateTime TagCreateTime)> EnumerateTagsOrderByNewer(DateTime from,int need_count);
    }
}
