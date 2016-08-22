using System;
using System.Text;
using DataAccess.Config;

namespace DataAccess.Common
{
    public class DataAccessException : ArgumentException
    {
        internal DataAccessException(Exception exception, CommandContent config)
            : base(BuildMessage(exception, config))
        { 
            
        }

        /// <summary>
        /// 这个还没有完成
        /// </summary>
        private static string BuildMessage(Exception exception, CommandContent config)
        {
            string errorMsg = "Data Command Exception";
            if (exception.InnerException != null)
                errorMsg = exception.InnerException.Message;

            StringBuilder msg = new StringBuilder();
            msg.AppendFormat("{0}\r\n", errorMsg);
            DataAccessSection section = new DataAccessSection();

            msg.AppendFormat("<<DataBase Name>> : {0}\r\n", config.DataBaseStr);
            if (section.ExceptionLevel == ExceptionLevel.Full)
                msg.AppendFormat("<<Connection String>> : {0}\r\n", "");

            msg.AppendFormat("<<Script Name>> : {0}\r\n", config.SqlName);
            if (section.ExceptionLevel == ExceptionLevel.Full)
                msg.AppendFormat("<<SQL Script>> : {0}\r\n", "");
            
            //如果有参数最好将参数也返回
            return msg.ToString();
        }
    }
}
