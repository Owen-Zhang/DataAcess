using Dapper;
using System.Data;

namespace DataAccess
{
    public class MultipleReader
    {
        private IDbConnection connection;

        internal MultipleReader(IDbCommand command, IDataReader reader, SqlMapper.Identity identity, DynamicParameters dynamicParams, bool addToCache, IDbConnection connection)
        {
            this.connection = connection;
        }

        public void Dispose()
        {
            if (connection != null)
                connection.Close();
        }
    }
}
