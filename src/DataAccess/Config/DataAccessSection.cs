using System;
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

        [ConfigurationProperty("ExceptionLevel", IsRequired =false)]
        public ExceptionLevel ExceptionLevel {
            get
            {
                ExceptionLevel temp;
                if (Enum.TryParse(this["ExceptionLevel"].ToString(), out temp))
                    return temp;
                else return ExceptionLevel.Full;
            }
        }
    }

    public enum ExceptionLevel {
        Full,
        Safety
    }
}
