using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Galleries;
using Wbooru.Models;
using Wbooru.Models.Gallery;

namespace Wbooru.Persistence
{
    public class LocalDBContext:DbContext
    {
        private static LocalDBContext _instance;

        public static LocalDBContext Instance => _instance ?? (_instance = new LocalDBContext());

        public DbSet<Download> Downloads { get; set; }
        public DbSet<TagRecord> Tags { get; set; }
        public DbSet<ShadowGalleryItem> ShadowGalleryItems { get; set; }
        public DbSet<VisitRecord> VisitRecords { get; set; }
        public DbSet<GalleryItemMark> ItemMarks { get; set; }
    }
}
