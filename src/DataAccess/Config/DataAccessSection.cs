using System.Configuration;

namespace DataAccess.Config
{
    public class DataAccessSection : ConfigurationSection
    {
        [ConfigurationProperty("DbCommandFilePath", IsRequired = true)]
        public string DbCommandFilePath
        {
            get {
                return this["DbCommandFilePath"] as string;
            }
        }

        [ConfigurationProperty("DataBaseFilePath", IsRequired = true)]
        public string DataBaseFilePath
        {
            get {
                return this["DataBaseFilePath"] as string;
            }
        }
    }
}
