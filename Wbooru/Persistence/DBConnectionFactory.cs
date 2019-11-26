using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            return _cached_connection = new SQLiteConnection()
            {
                ConnectionString = new SQLiteConnectionStringBuilder()
                {
                    DataSource = "H:\\WbooruDB.db",
                    ForeignKeys = true
                }.ConnectionString
            };
#else
            return _cached_connection = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
#endif
        }
    }
}
