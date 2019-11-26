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

        public static DbConnection GetConnection()
        {
            if (_cached_connection != null)
                return _cached_connection;

#if SQLITE_DEBUG || SQLITE
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
    }
}
