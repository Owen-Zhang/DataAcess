namespace DataAccess.Common
{
    /// <summary>
    /// 外部使用都要实例此接口
    /// </summary>
    public interface IDataAccessLog
    {
        void Info(object content);
        void InfoFormat(string format, params object[] content);
        void Error(object content);
        void ErrorFormat(string format, params object[] conent);
    }

    /// <summary>
    /// 空实现
    /// </summary>
    public class NullLog : IDataAccessLog
    {
        public void Info(object content)
        {
        }

        public void InfoFormat(string format, params object[] content)
        {
        }

        public void Error(object content)
        {
        }

        public void ErrorFormat(string format, params object[] conent)
        {
        }
    }

    public class LogManager
    {
        private static IDataAccessLog log;

        /// <summary>
        /// 外部使用者设置实例化的log
        /// </summary>
        /// <param name="logTemp"></param>
        public static void SetDataAccessLog(IDataAccessLog logTemp)
        {
            log = logTemp;
        }

        /// <summary>
        /// DataAcccess Layer 使用记日志
        /// </summary>
        internal static IDataAccessLog Log
        {
            get {
                if (log == null)
                    log = new NullLog();
                return log;
            }
        }
    }
}
