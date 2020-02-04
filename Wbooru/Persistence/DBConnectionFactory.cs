using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Settings;

namespace Wbooru.Persistence
{
    public static class DBConnectionFactory
    {
        private static DbConnection _cached_connection;

        public static DbConnection GetConnection()
        {
            if (_cached_connection != null)
                return _cached_connection;

            PreApplyDBFileIfNotExist();

            var db_file_path = Path.GetFullPath(SettingManager.LoadSetting<GlobalSetting>().DBFilePath);

#if DEBUG
            db_file_path = File.Exists(db_file_path) ? db_file_path : @"F:\Wbooru\data.db";
#endif

            Log.Info($"Use sqlite db , db file :{db_file_path}");

            return _cached_connection = new SQLiteConnection()
            {
                ConnectionString = new SQLiteConnectionStringBuilder()
                {
                    DataSource = db_file_path,
                    ForeignKeys = true
                }.ConnectionString
            };
        }

        private static void PreApplyDBFileIfNotExist()
        {
            var path = Path.GetFullPath(SettingManager.LoadSetting<GlobalSetting>().DBFilePath);

            if (File.Exists(path))
                return;

            //try get default empty database file
            var exe_path = Directory.GetParent(Process.GetCurrentProcess().MainModule.FileName).FullName;
            var default_db_path = Path.Combine(exe_path,"default_empty.db");

            if (!File.Exists(default_db_path))
                throw new Exception($"Wbooru没有数据库，也没从{default_db_path}找到default_empty.db以及其他的数据库文件资源，请自行生成数据库文件并放置到{path}.");

            Log.Info($"user database file not found , move default database file {default_db_path} to {path}");
            File.Copy(default_db_path, path);
        }
    }
}
