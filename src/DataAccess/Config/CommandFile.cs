using System.Collections.Generic;
using System.Data;
using System.Xml.Serialization;

namespace DataAccess.Config
{
    [XmlRoot("CommandFile")]
    public class CommandFile
    {
        [XmlArrayItem("Command")]
        public List<CommandContent> CommandList { get; set; }
    }

    public class CommandContent
    {
        private int timeOut = 300;
        private CommandType commandType = CommandType.Text;
        private string commandText = string.Empty;

        [XmlAttribute("Name")]
        public string SqlName { get; set; }

        [XmlAttribute("DataBase")]
        public string DataBaseStr { get; set; }

        [XmlAttribute("CommandType")]
        public CommandType CommandType
        {
            get
            {
                return this.commandType;
            }
            set
            {
                commandType = value;
            }
        }

        [XmlElement("CommandText")]
        public string CommandText
        {
            get
            {
                return string.IsNullOrEmpty(commandText) ? string.Empty : commandText.Trim();
            }
            set
            {
                commandText = value;
            }
        }

        [XmlAttribute("TimeOut")]
        public int TimeOut
        {
            get
            {
                return timeOut;
            }
            set
            {
                timeOut = value;
            }
        }

        [XmlArrayItem("Parameter")]
        public List<Parameter> Parameters { get; set; }
    }

    public class Parameter
    {
        private int size = -1;
        private ParameterDirection direction = ParameterDirection.Input;

        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("DbType")]
        public DbType DbType { get; set; }

        [XmlAttribute("Direction")]
        public ParameterDirection Direction
        {
            get
            {
                return direction;
            }
            set
            {
                this.direction = value;
            }
        }

        [XmlAttribute("Size")]
        public int Size
        {
            get {
                return size;
            }
            set {
                size = value;
            }
        }
    }
}

