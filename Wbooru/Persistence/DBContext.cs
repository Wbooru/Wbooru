using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Galleries;

namespace Wbooru.Persistence
{
    [Export(typeof(LocalDBContext))]
    public class LocalDBContext:DbContext
    {
        public DbSet<DownloadRecord> DownloadRecords { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<VisitRecord> VisitRecords { get; set; }
        public DbSet<GalleryItemMark> ItemMarks { get; set; }
    }
}
