using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SQLite;
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

#if SQLITE_DEBUG || SQLITE
        public static bool UsingSqlite => true;
#else
        public static bool UsingSqlite => false;
#endif

        public static DbConnection GetConnection()
        {
            if (_cached_connection != null)
                return _cached_connection;

#if SQLITE_DEBUG || SQLITE
            PreApplyDBFileIfNotExist();

            var db_file_path = Path.GetFullPath(SettingManager.LoadSetting<GlobalSetting>().DBFilePath);
            Log.Info($"Use sqlite db , db file :{db_file_path}");

            return _cached_connection = new SQLiteConnection()
            {
                ConnectionString = new SQLiteConnectionStringBuilder()
                {
                    DataSource = db_file_path,
                    ForeignKeys = true
                }.ConnectionString
            };
#else
            Log.Info($"Use localdb");
            return _cached_connection = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
#endif
        }

        private static void PreApplyDBFileIfNotExist()
        {
            var path = Path.GetFullPath(SettingManager.LoadSetting<GlobalSetting>().DBFilePath);

            if (File.Exists(path))
                return;

            //try get default empty database file
            var default_db_path = "default_empty.db";

            if (!File.Exists(default_db_path))
                throw new Exception("Wbooru没有数据库，也没找到default_empty.db以及其他的数据库文件资源，请自行生成数据库文件.");

            Log.Info($"user database file not found , move default database file {default_db_path} to {path}");
            File.Copy(default_db_path, path);
        }
    }
}
