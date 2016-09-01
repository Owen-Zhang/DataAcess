using DataAccess.Config;
using System.Collections.Generic;

namespace DataAccess.Main
{
    /// <summary>
    /// 1： 异常处理
    /// 2： 处理日志
    /// 3:  IDbConnection对象缓存
    /// </summary>
    public class DbManager
    {
        /// <summary>
        /// 加载数据访问层的相关配制(最好放在主程序启动里)
        /// </summary>
        public static void LoadDataAccessConfig()
        {
            ConfigFileManager.LoadDataAccessFile();
        }

        /// <summary>
        /// 获取一个运行时的Command
        /// </summary>
        public static DataCommand GetDataCommand(string sqlNodeName)
        {
            return new DataCommand(ConfigFileManager.GetSqlContentInfo(sqlNodeName));
        }
    }
}
