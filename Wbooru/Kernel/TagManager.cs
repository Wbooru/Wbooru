using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Models;
using Wbooru.Persistence;
using Wbooru.Utils;

namespace Wbooru.Kernel
{
    public static class TagManager
    {
        public static ObservableCollection<TagRecord> MarkedTags { get; private set; }
        public static ObservableCollection<TagRecord> FiltedTags { get; private set; }

        public static void InitTagManager()
        {
            MarkedTags = new ObservableCollection<TagRecord>(LocalDBContext.Instance.Tags.Where(x => !x.IsFilter));
            FiltedTags = new ObservableCollection<TagRecord>(LocalDBContext.Instance.Tags.Where(x => x.IsFilter));
        }

        public static void AddTag(string name, string gallery_name, string type, bool is_filter) => AddTag(name, gallery_name, Enum.TryParse<TagType>(type, true, out var r) ? r : TagType.Unknown, is_filter);

        public static bool Contain(string tag_name, bool is_filter) => (is_filter ? FiltedTags : MarkedTags).Any(x => x.Tag.Name.Equals(tag_name, StringComparison.InvariantCultureIgnoreCase));

        public static void AddTag(string name, string gallery_name, TagType type, bool is_filter) => AddTag(new Tag()
        {
            Name = name,
            Type = type
        }, gallery_name, is_filter);

        public static void AddTag(Tag tag_name, string gallery_name, bool is_filter)
        {
            var rt = is_filter ? FiltedTags : MarkedTags;

            TagRecord tag = new TagRecord()
            {
                Tag = tag_name,
                TagID = MathEx.Random(max: -1),
                AddTime = DateTime.Now,
                FromGallery = gallery_name,
                IsFilter = is_filter
            };

            LocalDBContext.Instance.Tags.Add(tag);
            LocalDBContext.Instance.SaveChanges();

            rt.Add(tag);
        }

        public static void RemoveTag(TagRecord record)
        {
            var list = record.IsFilter ? FiltedTags : MarkedTags;
            var need_delete = LocalDBContext.Instance.Tags.Where(x => x.IsFilter == record.IsFilter && x.Tag.Name.Equals(record.Tag.Name, StringComparison.InvariantCultureIgnoreCase)).ToArray();
            LocalDBContext.Instance.Tags.RemoveRange(need_delete);
            LocalDBContext.Instance.SaveChanges();

            foreach (var tag in need_delete.Select(x => x.Tag))
            {
                list.Remove(list.Single(x=>x.Tag.Name==tag.Name&&x.Tag.Type==tag.Type));
            }
        }
    }
}
