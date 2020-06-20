using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Wbooru.Galleries;
using Wbooru.Galleries.SupportFeatures;
using Wbooru.Models;
using Wbooru.Persistence;
using Wbooru.Settings;
using Wbooru.Utils;
using static Wbooru.Models.TagRecord;
using static System.Linq.Enumerable;

namespace Wbooru.Kernel
{
    public static class TagManager
    {
        public static ObservableCollection<TagRecord> MarkedTags { get; private set; }
        public static ObservableCollection<TagRecord> FiltedTags { get; private set; }
        public static ObservableCollection<TagRecord> SubscribedTags { get; private set; }

        public static void InitTagManager()
        {
            try
            {
                MarkedTags = new ObservableCollection<TagRecord>(order(LocalDBContext.Instance.Tags.AsNoTracking().Where(x => x.RecordType.HasFlag(TagRecordType.Marked))));
                FiltedTags = new ObservableCollection<TagRecord>(order(LocalDBContext.Instance.Tags.AsNoTracking().Where(x => x.RecordType.HasFlag(TagRecordType.Filter))));
                SubscribedTags = new ObservableCollection<TagRecord>(order(LocalDBContext.Instance.Tags.AsNoTracking().Where(x => x.RecordType.HasFlag(TagRecordType.Subscribed))));

                IEnumerable<TagRecord> order(IEnumerable<TagRecord> source)
                {
                    if (Setting<GlobalSetting>.Current.TagListViewerListOrder == GlobalSetting.TagListOrder.AddedDateTime)
                        return source.OrderBy(x => x.AddTime);
                    else
                        return source.OrderBy(x => x.Tag.Name);
                }
            }
            catch (Exception e)
            {
                ExceptionHelper.DebugThrow(e);
                Log.Error("Cant get tags from database.", e);
            }
        }

        //因为从现有的收藏标签进行订阅，所以不需要其他重载形式
        public static void SubscribedTag(TagRecord tag)
        {
            if (Contain(tag.Tag.Name,tag.FromGallery,TagRecordType.Subscribed))
                return;

            LocalDBContext.Instance.Attach(tag).Entity.RecordType = TagRecordType.Subscribed;

            LocalDBContext.Instance.SaveChanges();

            SubscribedTags.Add(tag);
        }

        //因为从现有的收藏标签进行订阅，所以不需要其他重载形式
        public static void UnSubscribedTag(TagRecord tag)
        {
            if (!tag.RecordType.HasFlag(TagRecordType.Subscribed))
                return;

            LocalDBContext.Instance.Attach(tag).Entity.RecordType = TagRecordType.Marked;

            LocalDBContext.Instance.SaveChanges();

            SubscribedTags.Remove(SubscribedTags.FirstOrDefault(x => x.Tag.Name == tag.Tag.Name && x.FromGallery == tag.FromGallery && x.Tag.Type == tag.Tag.Type));
        }

        public static void AddTag(string name, string gallery_name, string type, TagRecordType record_type) => AddTag(name, gallery_name, Enum.TryParse<TagType>(type, true, out var r) ? r : TagType.Unknown, record_type);

        public static bool Contain(string tag_name,string gallery_name, TagRecordType record_type) => (record_type switch
        {
            TagRecordType.Filter => FiltedTags,
            TagRecordType.Subscribed => SubscribedTags,
            TagRecordType.Marked => MarkedTags,
            _ => throw new Exception("咕咕")
        }).Any(x => x.Tag.Name.Equals(tag_name, StringComparison.InvariantCultureIgnoreCase) && gallery_name == x.FromGallery);

        public static void AddTag(string name, string gallery_name, TagType type, TagRecordType record_type) => AddTag(new Tag()
        {
            Name = name,
            Type = type
        }, gallery_name, record_type);

        public static void AddTag(Tag tag, string gallery_name, TagRecordType record_type)
        {
            var rt = record_type switch
            {
                TagRecordType.Filter => FiltedTags,
                TagRecordType.Subscribed => SubscribedTags,
                TagRecordType.Marked => MarkedTags,
                _ => throw new Exception("咕咕")
            };

            var tag_name = tag.Name;

            if (LocalDBContext.Instance.Tags.FirstOrDefault(x => tag_name == x.Tag.Name && gallery_name == x.FromGallery) is TagRecord record)
            {
                record.RecordType = record_type;
                rt.Add(record);
            }
            else
            {
                TagRecord tag2 = new TagRecord()
                {
                    Tag = tag,
                    TagID = MathEx.Random(max: -1),
                    AddTime = DateTime.Now,
                    FromGallery = gallery_name,
                    RecordType = record_type
                };

                LocalDBContext.Instance.Tags.Add(tag2);
                rt.Add(tag2);
            }

            LocalDBContext.Instance.SaveChanges();
        }

        public static void RemoveTag(TagRecord record)
        {
            var list = record.RecordType.HasFlag(TagRecordType.Filter) ? FiltedTags : MarkedTags;
            var tag = LocalDBContext.Instance.Tags.FirstOrDefault(x => x.RecordType == record.RecordType && x.FromGallery == record.FromGallery && x.Tag.Name.Equals(record.Tag.Name, StringComparison.InvariantCultureIgnoreCase));

            list.Remove(list.FirstOrDefault(x => x.Tag.Name == tag.Tag.Name && x.Tag.Type == tag.Tag.Type));

            if (tag.RecordType == TagRecordType.Subscribed)
                SubscribedTags.Remove(list.FirstOrDefault(x => x.Tag.Name == tag.Tag.Name && x.Tag.Type == tag.Tag.Type));

            tag.RecordType = TagRecordType.None;

            LocalDBContext.Instance.SaveChanges();
        }

        public static CacheTagMetaProgressStatus StartCacheTagMeta()
        {
            CacheTagMetaProgressStatus status = new CacheTagMetaProgressStatus();

            status.Task = Task.Run(() =>
            {
                var search_list = Container.Default.GetExportedValues<Gallery>().OfType<IGalleryTagMetaSearch>().ToArray();
                status.SearchCount = search_list.Length + 1;

                foreach (var searcher in search_list)
                {
                    status.CurrentSearchingName = (searcher as Gallery)?.GalleryName;

                    var list = searcher.StartPreCacheTags().MakeMultiThreadable();
                    int taked = 0;
                    const int need = 20;

                    var tags = list.Skip(taked).Take(need).ToArray();

                    while (tags.Any())
                    {
                        if (status.RequestCancel)
                            return;

                        //process tags group
                        ProcessTags(tags, searcher);

                        taked += tags.Count();
                        status.AddedCount = taked;
                        tags = list.Skip(taked).Take(need).ToArray();
                        Log.Debug($"skiped({taked}) taked({need}) actual_taked{tags.Length}");
                    }

                    status.FinishedCount++;
                }
            });

            return status;
        }

        private static void ProcessTags(Tag[] tags, IGalleryTagMetaSearch searcher)
        {
            var ctx = LocalDBContext.Instance;
            var gallery_name = (searcher as Gallery)?.GalleryName;
            using var u = ctx.Database.BeginTransaction();

            foreach (var tag in tags)
            {
                if (ctx.Tags.FirstOrDefault(x => x.Tag.Name == tag.Name && x.FromGallery == gallery_name) is TagRecord record)
                {
                    record.Tag.Type = tag.Type;
                    Log.Debug($"Modify tag record ({record.TagID}){record.Tag.Name} type = {record.Tag.Type}");
                }
                else
                {
                    record = new TagRecord()
                    {
                        TagID = MathEx.Random(max: -1),
                        Tag = tag,
                        RecordType = TagRecordType.None,
                        FromGallery = gallery_name,
                        AddTime = DateTime.Now
                    };
                    ctx.Tags.Add(record);

                    Log.Debug($"Add new tag record {record.Tag.Name} type = {record.Tag.Type}");
                }

                ctx.SaveChanges();
            }

            u.Commit();
        }

        public static Dictionary<string, Tag> SearchTagMeta(Gallery gallery = null, string id = null, params string[] tag_names)
        {
            Log.Debug($"+ Begin search({tag_names.Count()}): {string.Join(" , ", tag_names)}");

            var final_result = SearchTagMetaFromLocalDataBase(gallery, tag_names);

            if (final_result.Count != tag_names.Length)
            {
                var searchers = gallery == null ? Container.Default.GetExportedValues<Gallery>() : new[] { gallery };
                var tasks = searchers.OfType<IGalleryTagMetaSearch>()
                    .Select(searcher => Task.Run(() =>
                    {
                        try
                        {
                            var rid = !string.IsNullOrWhiteSpace(id) ? searcher.SearchTagMetaById(id).ToArray() : Enumerable.Empty<Tag>();
                            var sid = rid.Count() == tag_names.Length ? Enumerable.Empty<Tag>() : searcher.SearchTagMeta(tag_names.Except(rid.Select(x => x.Name)).ToArray());

                            return rid.Concat(sid).ToDictionary(x => x.Name, x => x);
                        }
                        catch (Exception e)
                        {
                            Log.Error($"Cant seaarch tags ({string.Join(",", tag_names)}) from gallery {(searcher as Gallery).GalleryName} : {e.Message}");
                            return new Dictionary<string, Tag>();
                        }
                    })).ToArray();

                Task.WaitAll(tasks);

                var search_results = tasks.Select(x => x.Result).SelectMany(x => x.Values).Distinct().ToArray();

                //got new tag meta by gallery/network search and save them.
                if (Setting<GlobalSetting>.Current.PredownloadAndCacheTagData)
                    UpdateTagMeta(search_results, gallery);

                Log.Debug($"? gallery search({search_results.Length}): {string.Join(" , ", search_results.Select(x => x.Name))}");

                foreach (var pair in search_results.ToDictionary(x => x.Name, y => y))
                    final_result[pair.Key] = pair.Value;
            }

            Log.Debug($"- End search({final_result.Count}): {string.Join(" , ", final_result.Keys)}");
            return final_result;
        }

        public static void UpdateTagMeta(IEnumerable<Tag> tags,Gallery gallery)
        {
            if (gallery == null)
                return;

            var gallery_name = gallery.GalleryName;

            var tag_names = tags.Select(x => x.Name).ToArray();

            var exist_record = (from record in
                             (from record in LocalDBContext.Instance.Tags.AsNoTracking()
                              where tag_names.Contains(record.Tag.Name)
                              where record.Tag.Type != TagType.Unknown
                              select record)
                         where gallery_name == record.FromGallery
                         select record).ToArray();

            foreach (var r in exist_record)
                LocalDBContext.Instance.Attach(r).Entity.Tag.Type = tags.FirstOrDefault(x => x.Name == r.Tag.Name).Type;

            var records = tags.Where(x=>!exist_record.Any(y=>y.Tag.Name == x.Name)).Select(x => new TagRecord()
            {
                TagID = MathEx.Random(max: -1),
                Tag = x,
                RecordType = TagRecordType.None,
                FromGallery = gallery_name,
                AddTime = DateTime.Now
            });

            LocalDBContext.Instance.Tags.AddRange(records);

            LocalDBContext.Instance.SaveChanges();
        }

        public static Dictionary<string, Tag> SearchTagMetaFromLocalDataBase(Gallery gallery = null,params string[] tag_names)
        {
            Log.Debug($"++ Begin search from database({tag_names.Length}): {string.Join(" , ", tag_names)}");

            var gallery_name = gallery?.GalleryName;
            var strict_check = (!Setting<GlobalSetting>.Current.SearchTagMetaStrict) || gallery_name == null;
        
            var result = from record in
                             (from record in LocalDBContext.Instance.Tags.AsNoTracking()
                              where tag_names.Contains(record.Tag.Name)
                              where record.Tag.Type != TagType.Unknown
                              select record)
                         where strict_check || gallery_name == record.FromGallery
                         select record;

            Dictionary<string, Tag> r = new Dictionary<string, Tag>();

            foreach (var record in result)
            {
                var name = record.Tag.Name;
                var tag = record.Tag;

                if (!r.ContainsKey(name) || /*优先替换同画廊定义的标签数据*/(gallery?.GalleryName) == name)
                {
                    r[name] = tag;
                    continue;
                }
            }

            Log.Debug($"-- End search from database({r.Count}): {string.Join(" , ", r.Keys)}");
            return r;
        }
    }
}
