using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;

namespace DataAccess.Common
{
    public class ConnectionFactory
    {
        public static IDbConnection GetConnection(string connectionStr, DbProvider dbProvider)
        {
            if (dbProvider == DbProvider.SqlServer)
                return new SqlConnection(connectionStr);
            else
                return new OleDbConnection(connectionStr);
        }
    }
}
