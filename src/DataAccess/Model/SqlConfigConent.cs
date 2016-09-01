using System.Data;
using Dapper;
using DataAccess.Common;
namespace DataAccess.Model
{
    public class SqlConfigConent
    {
        public DynamicParameters dapperParameters { get; set; }

        public CommandType CmdType { get; set; }

        public string SqlText {get;set;}

        public int Timeout { get; set; }

        public string ConnectionStr {get;set;}

        public DbProvider DbProvider { get; set; }
    }
}
