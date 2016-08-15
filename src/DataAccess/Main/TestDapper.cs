using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Main
{
    /// <summary>
    /// 通过IDConnection去扩展其它的数据访问方式
    /// </summary>
    public class TestDapper
    {
        public static void Test1(string connectionStr)
        {
            DynamicParameters paremeters = new DynamicParameters();
            paremeters.Add("@Name", 123);

            using (IDbConnection cnn = GetSqlConnection(connectionStr))
            {
                cnn.Execute("dbo.p_validateUser", paremeters, null, null, CommandType.StoredProcedure);
            }
        }

        private static IDbConnection GetSqlConnection(string connectionStr)
        {
            SqlConnection conn = new SqlConnection(connectionStr);
            conn.Open();
            return conn;
        }
    }
}
