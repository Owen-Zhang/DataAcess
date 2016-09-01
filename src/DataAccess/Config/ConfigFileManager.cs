using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using DataAccess.Common;

namespace DataAccess.Config
{
    public class ConfigFileManager
    {
        private static DataBaseInfo dataBaseInfo = null;
        private static List<CommandContent> sqlCommandList = null;

        /// <summary>
        /// 数据库连接相关的信息，connectionstring, Provider等
        /// </summary>
        public static DataBase GetDataBaseInfo(string baseName)
        {
            if (dataBaseInfo == null)
                LoadDataBaseInfo();
            
            return
            dataBaseInfo.DataBaseList.FirstOrDefault(
                item => string.Equals(item.Name, baseName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// sql 语句信息 
        /// </summary>
        public static CommandContent GetSqlContentInfo(string sqlName)
        {
            if (sqlCommandList == null)
                LoadCommandSqlInfo();

            return sqlCommandList.FirstOrDefault(item =>
                 string.Equals(item.SqlName, sqlName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// loadDataBaseInfo and Sql in host start
        /// </summary>
        public static void LoadDataAccessFile()
        {
            LoadDataBaseInfo();
            LoadCommandSqlInfo();
        }

        /// <summary>
        /// 此处要判断是否为相对地址还是绝对地址, 还没有处理, 如果是相对地址还要加上根路径
        /// </summary>
        private static void LoadDataBaseInfo()
        {
            var baseFilePath = DataAccessConfig.DataBaseFilePath;
            if (string.IsNullOrWhiteSpace(baseFilePath))
                throw new Exception("please config DataBase FilePath");

            baseFilePath = UtilTool.GetFilePath(baseFilePath);
            var fileContent = File.ReadAllText(baseFilePath);
            if (string.IsNullOrWhiteSpace(fileContent))
                throw new Exception("please config DataBase File Content");

            dataBaseInfo = UtilTool.XmlDeserialize<DataBaseInfo>(fileContent);
        }

        /// <summary>
        /// 此处要判断是否为相对地址还是绝对地址, 还没有处理, 如果是相对地址还要加上根路径
        /// </summary>
        private static void LoadCommandSqlInfo()
        {
            var sqlCommandPath = DataAccessConfig.DbCommandFilePath;
            if (string.IsNullOrWhiteSpace(sqlCommandPath))
                throw new Exception("please config sqlCommand FilePath");

            sqlCommandPath = UtilTool.GetFilePath(sqlCommandPath);
            var fileContent = File.ReadAllText(sqlCommandPath);

            if (string.IsNullOrWhiteSpace(fileContent))
                throw new Exception("DbCommandFiles is Empty");

            var sqlFile = UtilTool.XmlDeserialize<SqlFIleListInfo>(fileContent);
            sqlCommandList  = new List<CommandContent>();

            foreach (var item in sqlFile.SqlFileList)
            {
                if (string.IsNullOrWhiteSpace(item.Name)) continue;

                fileContent = File.ReadAllText(UtilTool.GetFilePath(item.Name));
                if (string.IsNullOrWhiteSpace(fileContent))
                    continue;

                sqlCommandList.AddRange(UtilTool.XmlDeserialize<CommandFile>(fileContent).CommandList);
            }
        }
    }
}
