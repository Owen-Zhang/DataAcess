using System;
using Dapper;
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
        public static int ExecuteNonQuery(SqlConfigConent sqlConfigConent)
        {
            IDbConnection connection = null;
            try
            {
                connection = GetConnection(sqlConfigConent.ConnectionStr, sqlConfigConent.DbProvider);
                connection.Open();

                return SqlMapper.Execute(
                            connection, 
                            sqlConfigConent.SqlText,
                            sqlConfigConent.dapperParameters, 
                            commandTimeout: sqlConfigConent.Timeout, 
                            commandType:sqlConfigConent.CmdType);
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
        public static T ExecuteScalar<T>(SqlConfigConent sqlConfigConent)
        {
            IDbConnection connection = null;
            try
            {
                connection = GetConnection(sqlConfigConent.ConnectionStr, sqlConfigConent.DbProvider);
                connection.Open();

                return SqlMapper.ExecuteScalar<T>(
                            connection,
                            sqlConfigConent.SqlText,
                            sqlConfigConent.dapperParameters,
                            commandTimeout: sqlConfigConent.Timeout,
                            commandType: sqlConfigConent.CmdType);
            }
            finally {
                if (connection.State != ConnectionState.Closed)
                    connection.Close();
            }
        }

        public static T QuerySingle<T>(SqlConfigConent sqlConfigConent)
        {
            IDbConnection connection = null;
            try
            {
                connection = GetConnection(sqlConfigConent.ConnectionStr, sqlConfigConent.DbProvider);
                connection.Open();

                return SqlMapper.QuerySingle<T>(
                            connection,
                            sqlConfigConent.SqlText,
                            sqlConfigConent.dapperParameters,
                            commandTimeout: sqlConfigConent.Timeout,
                            commandType: sqlConfigConent.CmdType);
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                    connection.Close();
            }
        }

        /// <summary>
        /// 返回单个实体数据
        /// 这个业务有很多的测试用例
        /// https://github.com/StackExchange/dapper-dot-net/blob/bffb0972a076734145d92959dabbe48422d12922/Dapper.Tests/Tests.cs
        /// </summary>
        public static T Query<T>(SqlConfigConent sqlConfigConent)
        {
            IDbConnection connection = null;
            try
            {
                connection = GetConnection(sqlConfigConent.ConnectionStr, sqlConfigConent.DbProvider);
                connection.Open();

                return SqlMapper.QueryFirstOrDefault<T>(
                            connection,
                            sqlConfigConent.SqlText,
                            sqlConfigConent.dapperParameters,
                            commandTimeout: sqlConfigConent.Timeout,
                            commandType: sqlConfigConent.CmdType);
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
        public static List<T> QueryList<T>(SqlConfigConent sqlConfigConent)
        {
            IDbConnection connection = null;
            try
            {
                connection = GetConnection(sqlConfigConent.ConnectionStr, sqlConfigConent.DbProvider);
                connection.Open();

                return SqlMapper.Query<T>(
                            connection,
                            sqlConfigConent.SqlText,
                            sqlConfigConent.dapperParameters,
                            commandTimeout: sqlConfigConent.Timeout,
                            commandType: sqlConfigConent.CmdType).ToList();
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
        public static Dapper.SqlMapper.GridReader QueryMultiple(SqlConfigConent sqlConfigConent)
        {
            var connection = GetConnection(sqlConfigConent.ConnectionStr, sqlConfigConent.DbProvider);
            connection.Open();

            return SqlMapper.QueryMultiple(
                            connection,
                            sqlConfigConent.SqlText,
                            sqlConfigConent.dapperParameters,
                            commandTimeout: sqlConfigConent.Timeout,
                            commandType: sqlConfigConent.CmdType);
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
