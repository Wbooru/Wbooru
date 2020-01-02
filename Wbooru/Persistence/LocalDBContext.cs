using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Galleries;
using Wbooru.Models;
using Wbooru.Models.Gallery;
using Wbooru.Settings;

namespace Wbooru.Persistence
{
    public class LocalDBContext:DbContext
    {
        private static LocalDBContext _instance;

        public static LocalDBContext Instance => _instance ?? (_instance = new LocalDBContext());

        public LocalDBContext() : base(DBConnectionFactory.GetConnection(),true)
        {

        }

        public DbSet<Download> Downloads { get; set; }
        public DbSet<TagRecord> Tags { get; set; }
        public DbSet<ShadowGalleryItem> ShadowGalleryItems { get; set; }
        public DbSet<VisitRecord> VisitRecords { get; set; }
        public DbSet<GalleryItemMark> ItemMarks { get; set; }

        internal static void BackupDatabase(string to)
        {
            if (!DBConnectionFactory.UsingSqlite)
            {
                Log.Info("Support sqlite only.");
                return;
            }

            var from = SettingManager.LoadSetting<GlobalSetting>().DBFilePath;
            Log.Info($"Copy sqlite db file from {from} to {to}");

            File.Copy(from, to, true);
        }

        internal static void RestoreDatabase(string from, string to)
        {
            if (!DBConnectionFactory.UsingSqlite)
            {
                Log.Info("Support sqlite only.");
                return;
            }

            Log.Info($"Copy sqlite db file from {from} to {to}");

            File.Copy(from, to, true);
        }
    }
}
