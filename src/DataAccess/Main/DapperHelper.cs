using System;
using DataAccess;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using DataAccess.Common;
using DataAccess.Model;

namespace DataAccess.Main
{
    /// <summary>
    /// 原则上是包一层， 这样便于好升级Dapper
    /// </summary>
    public class DapperHelper
    {
        /// <summary>
        /// 不需要有返回值
        /// </summary>
        public static int ExecuteNonQuery(SqlConfigContent sqlConfigContent)
        {
            IDbConnection connection = null;
            try
            {
                connection = GetConnection(sqlConfigContent.ConnectionStr, sqlConfigContent.DbProvider);
                connection.Open();

                return SqlMapper.Execute(
                            connection,
                            sqlConfigContent.SqlText,
                            sqlConfigContent.dapperParameters,
                            commandTimeout: sqlConfigContent.Timeout,
                            commandType: sqlConfigContent.CmdType);
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                    connection.Close();
            }
        }

        /// <summary>
        /// 返回单个值
        /// </summary>
        public static T ExecuteScalar<T>(SqlConfigContent sqlConfigContent)
        {
            IDbConnection connection = null;
            try
            {
                connection = GetConnection(sqlConfigContent.ConnectionStr, sqlConfigContent.DbProvider);
                connection.Open();

                return SqlMapper.ExecuteScalar<T>(
                            connection,
                            sqlConfigContent.SqlText,
                            sqlConfigContent.dapperParameters,
                            commandTimeout: sqlConfigContent.Timeout,
                            commandType: sqlConfigContent.CmdType);
            }
            finally {
                if (connection.State != ConnectionState.Closed)
                    connection.Close();
            }
        }

        /* 暂时不用
        public static T QuerySingle<T>(SqlConfigContent sqlConfigContent)
        {
            IDbConnection connection = null;
            try
            {
                connection = GetConnection(sqlConfigContent.ConnectionStr, sqlConfigContent.DbProvider);
                connection.Open();

                return SqlMapper.QuerySingle<T>(
                            connection,
                            sqlConfigContent.SqlText,
                            sqlConfigContent.dapperParameters,
                            commandTimeout: sqlConfigContent.Timeout,
                            commandType: sqlConfigContent.CmdType);
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                    connection.Close();
            }
        }*/ 

        /// <summary>
        /// 返回单个实体数据
        /// 这个业务有很多的测试用例
        /// https://github.com/StackExchange/dapper-dot-net/blob/bffb0972a076734145d92959dabbe48422d12922/Dapper.Tests/Tests.cs
        /// </summary>
        public static T Query<T>(SqlConfigContent sqlConfigContent)
        {
            IDbConnection connection = null;
            try
            {
                connection = GetConnection(sqlConfigContent.ConnectionStr, sqlConfigContent.DbProvider);
                connection.Open();

                var result = SqlMapper.Query<T>(
                            connection,
                            sqlConfigContent.SqlText,
                            sqlConfigContent.dapperParameters,
                            commandTimeout: sqlConfigContent.Timeout,
                            commandType: sqlConfigContent.CmdType);
                if (result == null || result.Count() == 0)
                    return default(T);
                else
                    return result.FirstOrDefault();
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                    connection.Close();
            }
        }

        /// <summary>
        /// 返回多条数据的List, 还可以返回(List<int>这种数据)
        /// </summary>
        public static List<T> QueryList<T>(SqlConfigContent sqlConfigContent)
        {
            IDbConnection connection = null;
            try
            {
                connection = GetConnection(sqlConfigContent.ConnectionStr, sqlConfigContent.DbProvider);
                connection.Open();

                return SqlMapper.Query<T>(
                            connection,
                            sqlConfigContent.SqlText,
                            sqlConfigContent.dapperParameters,
                            commandTimeout: sqlConfigContent.Timeout,
                            commandType: sqlConfigContent.CmdType).ToList();
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                    connection.Close();
            }
        }

        /// <summary>
        /// 多个返回实体, 这个方法没有关闭connection, 需要手动关闭connection
        /// </summary>
        public static GridReader QueryMultiple(SqlConfigContent sqlConfigContent)
        {
            IDbConnection connection = null;
            try
            {

                connection = GetConnection(sqlConfigContent.ConnectionStr, sqlConfigContent.DbProvider);
                connection.Open();

                return SqlMapper.QueryMultiple(
                                connection,
                                sqlConfigContent.SqlText,
                                sqlConfigContent.dapperParameters,
                                commandTimeout: sqlConfigContent.Timeout,
                                commandType: sqlConfigContent.CmdType);
            }
            catch (Exception e)
            {
                if (connection != null)
                    connection.Close();
                throw e;
            }
        }

        /// <summary>
        /// 获取数据库的connection
        /// </summary>
        private static IDbConnection GetConnection(string connectionStr, DbProvider provider)
        {
            return ConnectionFactory.GetConnection(connectionStr, provider);
        }
    }
}
