using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using DataAccess.Common;

namespace DataAccess.Config
{
    public class ConfigFileManager
    {
        private static DataBaseInfo dataBaseInfo = new DataBaseInfo { DataBaseList = new List<DataBase>() };
        private static Dictionary<string, CommandContent> sqlCommandDic = new Dictionary<string, CommandContent>(500);

        /// <summary>
        /// 数据库连接相关的信息，connectionstring, Provider等
        /// </summary>
        public static DataBase GetDataBaseInfo(string baseName)
        {
            return
            dataBaseInfo.DataBaseList.FirstOrDefault(
                item => string.Equals(item.Name, baseName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// sql 语句信息 
        /// </summary>
        public static CommandContent GetSqlContentInfo(string sqlName)
        {
            CommandContent content;
            sqlCommandDic.TryGetValue(sqlName, out content);
            return content;
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
            {
                LogManager.Log.Error("please config DataBase FilePath");
                return;
            }

            baseFilePath = UtilTool.GetFilePath(baseFilePath);
            var fileContent = File.ReadAllText(baseFilePath);
            if (string.IsNullOrWhiteSpace(fileContent)) 
            {
                LogManager.Log.Error("please config DataBase File Content");
                return;
            }

            try
            {
                dataBaseInfo = UtilTool.XmlDeserialize<DataBaseInfo>(fileContent);
            }
            catch
            {
                LogManager.Log.Error("DataBase File content Deserialize failed");
                return;
            }
        }

        /// <summary>
        /// 此处要判断是否为相对地址还是绝对地址, 还没有处理, 如果是相对地址还要加上根路径
        /// </summary>
        private static void LoadCommandSqlInfo()
        {
            var sqlCommandPath = DataAccessConfig.DbCommandFilePath;
            if (string.IsNullOrWhiteSpace(sqlCommandPath))
            {
                LogManager.Log.Error("please config sqlCommand FilePath");
                return;
            }

            sqlCommandPath = UtilTool.GetFilePath(sqlCommandPath);
            var fileContent = File.ReadAllText(sqlCommandPath);
            if (string.IsNullOrWhiteSpace(fileContent))
            {
                LogManager.Log.Error("DbCommandFiles is Empty");
                return;
            }
SqlFIleListInfo sqlFile = null;
            try
            {
                sqlFile = UtilTool.XmlDeserialize<SqlFIleListInfo>(fileContent);
            }
            catch {
                LogManager.Log.Error("DbCommandFiles Deserialize faild");
                return;
            }

            var sqlFolderPath = Path.GetDirectoryName(sqlCommandPath);
            foreach (var item in sqlFile.SqlFileList)
            {
                if (string.IsNullOrWhiteSpace(item.Name)) continue;

                var realPath = UtilTool.GetFilePath(item.Name, sqlFolderPath);
                if (string.IsNullOrWhiteSpace(realPath) || !File.Exists(realPath))
                {
                    LogManager.Log.ErrorFormat("File:[{0}] not exists,please check", realPath);
                    continue;
                }
                fileContent = File.ReadAllText(realPath);
            
                if (string.IsNullOrWhiteSpace(fileContent))
                    continue;

                var commandList = new List<CommandContent>();
                try
                {
                    commandList.AddRange(UtilTool.XmlDeserialize<CommandFile>(fileContent).CommandList);
                }
                catch(Exception e)
                {
                    LogManager.Log.ErrorFormat("sqlFile Deserialize faild, fileName is : {0}, ErrorMsg: {1}", item.Name, e.Message);
                }

                commandList.ForEach(command => sqlCommandDic[command.SqlName] = command);
            }
        }

    }
}
