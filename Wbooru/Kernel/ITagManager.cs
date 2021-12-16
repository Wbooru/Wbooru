using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Galleries;
using Wbooru.Kernel.DI;
using Wbooru.Models;
using static Wbooru.Models.TagRecord;

namespace Wbooru.Kernel
{
    public interface ITagManager: IManagerLifetime, IImplementInjectable
    {
        ObservableCollection<TagRecord> MarkedTags { get; }
        ObservableCollection<TagRecord> FiltedTags { get; }
        ObservableCollection<TagRecord> SubscribedTags { get; }

        Task SubscribedTag(TagRecord tag);
        Task UnSubscribedTag(TagRecord tag);
        Task AddTag(Tag tag, string gallery_name, TagRecordType record_type);
        Task<bool> ContainTag(string tag_name, string gallery_name, TagRecordType record_type);
        Task RemoveTag(TagRecord tag);
        Task<Dictionary<string, Tag>> SearchTagMeta(Gallery gallery = null, string id = null, params string[] tag_names);
        CacheTagMetaProgressStatus StartCacheTagMeta();
    }
}
