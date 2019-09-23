using System.Data.Common;
using System.Data.Odbc;

namespace TrafficScraper.Data.Writer.Database
{
    public class OdbcDataWriter : DatabaseDataWriter
    {
        public OdbcDataWriter(string connectionString) : base(connectionString)
        {
        }

        public OdbcDataWriter(string connectionString, string tableName) : base(connectionString, tableName)
        {
        }

        protected override DbConnection CreateConnection()
        {
            return new OdbcConnection(connectionString);
        }
    }
}
