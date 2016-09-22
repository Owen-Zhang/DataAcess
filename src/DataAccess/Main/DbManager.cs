using DataAccess.Config;
using System.Collections.Generic;
using System;

namespace DataAccess
{
    /// <summary>
    /// 1： 异常处理
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
            var CommandContent = ConfigFileManager.GetSqlContentInfo(sqlNodeName);
            if (CommandContent == null)
                throw new Exception(string.Format("didn't find the sql config Node: {0}", sqlNodeName));

            return new DataCommand(CommandContent);
        }
    }
}
