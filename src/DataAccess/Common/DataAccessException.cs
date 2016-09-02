using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Common
{
    public class DataAccessException : Exception
    {
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public string ConnectionStr { private set; get; }

        /// <summary>
        /// 提交的sql语句
        /// </summary>
        public string SqlText { private set; get; }

        /// <summary>
        /// 参数
        /// </summary>
        public string ParameterStr { private set; get; }

        public DataAccessException(string errorMsg, string connectionStr = null, string sqlText = null, string parameterStr = null)
            : base(errorMsg)
        {
            ConnectionStr = connectionStr;
            SqlText = sqlText;
            ParameterStr = parameterStr;
        }
    }
}
