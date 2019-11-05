using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Models;

namespace Wbooru.Galleries.SupportFeatures
{
    public interface IGalleryTagDataPredownloadAndCache
    {
        IEnumerable<(Tag TagInfo,DateTime TagCreateTime)> EnumerateTagsOrderByNewer(DateTime from,int need_count);
    }
}
