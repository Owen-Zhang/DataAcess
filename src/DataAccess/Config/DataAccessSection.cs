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

        public ExceptionLevel ExceptionLevel {
            get
            {
                ExceptionLevel temp;
                if (Enum.TryParse(this["ExceptionLevel"] as string, out temp))
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
