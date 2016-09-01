using System.Collections.Generic;
using System.Xml.Serialization;
using DataAccess.Common;

namespace DataAccess.Config
{
    [XmlRoot("CommandFile")]
    public class DataBaseInfo
    {
        [XmlArrayItem("DataBase")]
        public List<DataBase> DataBaseList { get; set; }
    }

    public class DataBase
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("Type")]
        public DbProvider Type { get; set; }

        public string ConnectionString { get; set;}
    }
}
