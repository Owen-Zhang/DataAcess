using System.Configuration;

namespace DataAccess.Config
{
    public class DataAccessConfig
    {
        private static string dataAccessSection = "DataAccess";
        private static DataAccessSection accessSection =
            (DataAccessSection)ConfigurationManager.GetSection(dataAccessSection);

        public static string DbCommandFilePath
        {
            get {
                return accessSection.DbCommandFilePath;
            }
        }

        public static string DataBaseFilePath
        {
            get {
                return accessSection.DataBaseFilePath;
            }
        }
    }
}
