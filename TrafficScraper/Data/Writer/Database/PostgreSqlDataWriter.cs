using System.Data.Common;
using Npgsql;

namespace TrafficScraper.Data.Writer.Database
{
    public class PostgreSqlDataWriter : DatabaseDataWriter
    {
        public PostgreSqlDataWriter(string connectionString) : base(connectionString)
        {
        }

        public PostgreSqlDataWriter(string connectionString, string tableName) : base(connectionString, tableName)
        {
        }

        protected override DbConnection CreateConnection()
        {
            return new NpgsqlConnection(connectionString);
        }

        protected override string GetAutoIncrementKeyword()
        {
            return "SERIAL";
        }
    }
}
