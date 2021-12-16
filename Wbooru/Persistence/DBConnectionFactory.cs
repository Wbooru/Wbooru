using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Settings;

namespace Wbooru.Persistence
{
    public static class DBConnectionFactory
    {
        private static DbConnection _cached_connection;

        public static DbContextOptions GetDbContextOptions() => new DbContextOptionsBuilder().UseSqlite(GetConnection()).Options;

        public static DbConnection GetConnection()
        {
            if (_cached_connection != null)
                return _cached_connection;

            PreApplyDBFileIfNotExist();

            var db_file_path = Path.GetFullPath(Setting<GlobalSetting>.Current.DBFilePath);

#if DEBUG
            db_file_path = File.Exists(db_file_path) ? db_file_path : @"F:\Wbooru\data.db";
#endif

            Log.Info($"Use sqlite db , db file :{db_file_path}");

            return _cached_connection = new SQLiteConnection()
            {
                ConnectionString = new SQLiteConnectionStringBuilder()
                {
                    DataSource = db_file_path,
                    ForeignKeys = false
                }.ConnectionString
            };
        }

        private static void PreApplyDBFileIfNotExist()
        {
            var path = Path.GetFullPath(Setting<GlobalSetting>.Current.DBFilePath);

            if (File.Exists(path))
                return;

            using var stream = typeof(App).Assembly.GetManifestResourceStream("Wbooru.Persistence.default_empty.db");
            using var file = File.OpenWrite(path);
            stream.CopyTo(file);

            Log.Info($"user database file not found , generate new empty database file to {path}");
        }
    }
}
