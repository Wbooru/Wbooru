using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Models;

namespace Wbooru.Galleries.SupportFeatures
{
    public interface IGalleryTagMetaSearch : IGalleryFeature
    {
        IEnumerable<Tag> StartPreCacheTags();
        IEnumerable<Tag> SearchTagMetaById(string id);
        IEnumerable<Tag> SearchTagMeta(params string[] tags);
    }
}
