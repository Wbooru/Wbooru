using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Galleries;
using Wbooru.Kernel.DI;
using Wbooru.Models;
using Wbooru.Models.Gallery;
using Wbooru.Persistence;

namespace Wbooru.Kernel.ManagerImpl
{
    [PriorityExport(typeof(IMarkManager))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class MarkManager : IMarkManager
    {
        public Task SetMark(Gallery gallery, GalleryItem info, bool is_mark)
        {
            return LocalDBContext.PostDbAction(DB =>
            {
                if (is_mark)
                {
                    DB.ItemMarks.AddAsync(new GalleryItemMark()
                    {
                        GalleryItem = info,
                        Time = DateTime.Now
                    });
                }
                else
                {
                    var x = DB.ItemMarks.FirstOrDefault(x => x.GalleryItem.GalleryName == gallery.GalleryName && x.GalleryItem.GalleryItemID == info.GalleryItemID);
                    DB.ItemMarks.Remove(x);
                }

                DB.SaveChangesAsync();
            });
        }

        public Task<bool> GetMark(Gallery gallery, GalleryItem item)
        {
            return LocalDBContext.PostDbAction(DB =>
            {
                var visit_entity = DB.VisitRecords.AsQueryable().Where(x => x.GalleryItem != null).FirstOrDefault(x => x.GalleryItem.GalleryItemID == item.GalleryItemID && x.GalleryItem.GalleryName == gallery.GalleryName);

                if (visit_entity == null)
                {
                    var visit = new VisitRecord()
                    {
                        GalleryItem = item,
                        LastVisitTime = DateTime.Now
                    };

                    DB.VisitRecords.Add(visit);
                }
                else
                {
                    visit_entity.LastVisitTime = DateTime.Now;
                }

                DB.SaveChanges();

                return !(DB.ItemMarks.FirstOrDefault(x => x.GalleryItem.GalleryName == gallery.GalleryName && x.GalleryItem.GalleryItemID == item.GalleryItemID) is null);
            });
        }
    }
}
