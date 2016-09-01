using Dapper;
using System.Linq;
using DataAccess.Config;
using System.Collections.Generic;
using System.Data;
using DataAccess.Common;
using DataAccess.Model;
using System;

namespace DataAccess.Main
{
    /// <summary>
    /// 如果找不到那个配制的sql 调用方请抛出错误
    /// </summary>
    public class DataCommand
    {
        /// <summary>
        /// 由于QueryMultiple时，没有关闭connection, 在后面手动去关闭 
        /// </summary>
        private IDbConnection connection;

        private List<Parameter> configParemeterList;
        private SqlConfigConent sqlConfigConent;

        internal DataCommand(CommandContent config)
        {
            sqlConfigConent = new SqlConfigConent() { dapperParameters = new DynamicParameters() };
            configParemeterList = GetParametersFromConfig(config);
            AddReturnOrOutPutParameter();
        }

        /// <summary>
        /// 设置参数
        /// </summary>
        public void SetParameterValue(string parameterName, object value)
        {
            parameterName = CheckAndAddPre(parameterName);
            var configTemp = configParemeterList.FirstOrDefault(item => 
                string.Equals(item.Name, parameterName, System.StringComparison.OrdinalIgnoreCase));

            if (configTemp != null)
                sqlConfigConent.dapperParameters.Add(configTemp.Name, value, configTemp.DbType, configTemp.Direction, configTemp.Size);
        }

        /// <summary>
        /// 动态替换sql文本中的一些字符，如order by 字段名
        /// </summary>
        public void ReplaceSqlText(string oldString, string newString)
        {
            sqlConfigConent.SqlText = sqlConfigConent.SqlText.Replace(oldString, newString);
        }

        /// <summary>
        /// not return sqlDate, return effect rows 
        /// </summary>
        public int ExecuteNonQuery()
        {
            return DapperHelper.ExecuteNonQuery(sqlConfigConent);
        }

        /// <summary>
        /// 返回单个值
        /// </summary>
        public T ExecuteScalar<T>()
        {
            return DapperHelper.ExecuteScalar<T>(sqlConfigConent);
        }

        /// <summary>
        /// 返回单个实体数据
        /// </summary>
        public T Query<T>()
        {
            return DapperHelper.Query<T>(sqlConfigConent);
        }

        /// <summary>
        /// 返回集合数据
        /// </summary>
        public List<T> QueryList<T>()
        {
            return DapperHelper.QueryList<T>(sqlConfigConent);
        }

        /// <summary>
        /// 一次返回多个数据集，如(select * from a, select * from B.....)
        /// </summary>
        public Dapper.SqlMapper.GridReader QueryMultiple()
        {
            return DapperHelper.QueryMultiple(sqlConfigConent);
        }

        /// <summary>
        /// 关闭connection, 只在调用QueryMultiple方法时用
        /// </summary>
        public void CloseConnection()
        {
            if (connection != null)
                connection.Close();
        }

        /// <summary>
        /// 将输出参数和返回参数加到参数列表中
        /// </summary>
        private void AddReturnOrOutPutParameter()
        {
            configParemeterList.FindAll(item =>
                item.Direction == System.Data.ParameterDirection.Output ||
                item.Direction == System.Data.ParameterDirection.ReturnValue)
            .ForEach(temp => sqlConfigConent.dapperParameters.Add(temp.Name, null, temp.DbType, temp.Direction, temp.Size));
        }

        private List<Parameter> GetParametersFromConfig(CommandContent commandConfig)
        {
            sqlConfigConent.CmdType = commandConfig.CommandType;
            sqlConfigConent.SqlText = commandConfig.CommandText;
            sqlConfigConent.Timeout = commandConfig.TimeOut;

            string connectionStr; DbProvider dbProvider;
            GetDataBaseInfo(commandConfig.DataBaseStr, out connectionStr, out dbProvider);
            sqlConfigConent.ConnectionStr = connectionStr;
            sqlConfigConent.DbProvider = dbProvider;

            if (commandConfig.Parameters == null || commandConfig.Parameters.Count == 0)
                return new List<Parameter>();

            return commandConfig.Parameters.Select(item => 
                    new Parameter {
                        Name = item.Name,
                        DbType = item.DbType,
                        Direction = item.Direction,
                        Size = item.Size
                    }).ToList();
        }

        private string CheckAndAddPre(string paramName)
        {
            if (!paramName.StartsWith("@"))
                paramName = string.Format("@{0}", paramName);
            return paramName;
        }

        /// <summary>
        /// dataBaseName, sql文件中设置的数据库(此处只是一个映射名),
        /// </summary>
        /// <param name="dataBaseName"></param>
        public void GetDataBaseInfo(string dataBaseName, out string connectionStr, out DbProvider dbProvider)
        {
            var dataBase = ConfigFileManager.GetDataBaseInfo(dataBaseName);
            if (dataBase == null)
                throw new Exception(string.Format("please config [{0}] dataBase ConnectionString ", dataBaseName));

            connectionStr = dataBase.ConnectionString;
            dbProvider = dataBase.Type;
        }
    }
}
