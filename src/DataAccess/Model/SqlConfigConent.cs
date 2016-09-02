using System.Data;
using Dapper;
using DataAccess.Common;
using DataAccess.Config;
namespace DataAccess.Model
{
    public class SqlConfigContent
    {
        public DynamicParameters dapperParameters { get; set; }

        public CommandType CmdType { get; set; }

        public string SqlText {get;set;}

        public int Timeout { get; set; }

        public string ConnectionStr {get;set;}

        public DbProvider DbProvider { get; set; }

        public ExceptionLevel ExceptionLevel { get; set; }
    }
}
