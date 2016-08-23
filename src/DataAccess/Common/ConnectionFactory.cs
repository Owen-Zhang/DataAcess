using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;

namespace DataAccess.Common
{
    public class ConnectionFactory
    {
        /// <summary>
        /// 这个暂时这样写， 有空时复写(dbProvider写成Enum, 同时可以将实例缓存下来)
        /// </summary>
        public static IDbConnection GetConnection(string connectionStr, string dbProvider)
        {
            if (string.Equals("SqlServer", dbProvider, System.StringComparison.OrdinalIgnoreCase))
                return new SqlConnection(connectionStr);
            else
                return new OleDbConnection(connectionStr);
        }
    }
}
