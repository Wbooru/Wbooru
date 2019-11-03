using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Galleries;
using Wbooru.Models;

namespace Wbooru.Persistence
{
    [Export(typeof(LocalDBContext))]
    public class LocalDBContext:DbContext
    {
        public DbSet<Download> Downloads { get; set; }
        public DbSet<TagRecord> Tags { get; set; }
        public DbSet<VisitRecord> VisitRecords { get; set; }
        public DbSet<GalleryItemMark> ItemMarks { get; set; }
    }
}
