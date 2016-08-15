using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataAccess.Config
{
    [XmlRoot("CommandFile")]
    public class SqlFIleListInfo
    {
        [XmlArrayItem("File")]
        public List<FileInfo> SqlFileList { get; set; }
    }

    public class FileInfo
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }
    }
}
