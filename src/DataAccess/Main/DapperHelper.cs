using System;
using Dapper;
using System.Data;
using System.Linq;
using System.Collections.Generic;

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
        public static int ExecuteNonQuery(IDbConnection connection, CommandType cmdType, string sqlText, int timeout, dynamic parameters)
        {
            try
            {
                connection.Open();
                return SqlMapper.Execute(connection, sqlText, parameters, null, timeout, cmdType);
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
        public static T ExecuteScalar<T>(IDbConnection connection, CommandType cmdType, string sqlText, int timeout, dynamic parameters)
        {
            try
            {
                connection.Open();
                return SqlMapper.ExecuteScalar<T>(connection, sqlText, parameters, null, timeout, cmdType);
            }
            finally {
                if (connection.State != ConnectionState.Closed)
                    connection.Close();
            }
        }

        public static T QuerySingle<T>(IDbConnection connection, CommandType cmdType, string sqlText, int timeout, dynamic parameters)
        {
            try
            {
                connection.Open();
                return SqlMapper.QuerySingle<T>(connection, sqlText, parameters, null, timeout, cmdType);
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
        public static T Query<T>(IDbConnection connection, CommandType cmdType, string sqlText, int timeout, dynamic parameters)
        {
            try
            {
                connection.Open();
                return SqlMapper.QueryFirstOrDefault<T>(connection, sqlText, parameters, commandTimeout: timeout, commandType: cmdType);
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
        public static List<T> QueryList<T>(IDbConnection connection, CommandType cmdType, string sqlText, int timeout, dynamic parameters)
        {
            try
            {
                connection.Open();
                return SqlMapper.Query<T>(connection, sqlText, parameters, commandTimeout: timeout, commandType: cmdType).ToList();
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
        public static Dapper.SqlMapper.GridReader QueryMultiple<T>(IDbConnection connection, CommandType cmdType, string sqlText, int timeout, dynamic parameters)
        { 
            connection.Open();
            return SqlMapper.QueryMultiple(connection, sqlText, parameters, commandTimeout: timeout, commandType: cmdType);
        }
    }
}
