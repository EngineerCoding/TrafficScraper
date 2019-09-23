using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.Serialization;

namespace TrafficScraper.Data.Writer.Database
{
    [Serializable]
    class DatabaseException : Exception
    {
        public DatabaseException(string msg) : base(msg)
        {
        }

        public DatabaseException(string msg, Exception inner) : base(msg, inner)
        {
        }

        public DatabaseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }


    public abstract class DatabaseDataWriter : DataWriter
    {
        protected readonly string connectionString;
        protected readonly string tableName;

        public DatabaseDataWriter(string connectionString, string tableName = "TrafficJam") : base()
        {
            this.connectionString = connectionString;
            this.tableName = tableName;
        }

        protected abstract DbConnection CreateConnection();

        public override void WriteTrafficJams(List<TrafficJam> trafficJams)
        {
            using (DbConnection connection = CreateConnection())
            {
                connection.Open();
                try
                {
                    WriteToDatabase(connection, trafficJams);
                }
                catch (Exception e)
                {
                    if (!(e is DatabaseException))
                        throw new DatabaseException("Unexpected database error", e);
                    throw;
                }
            }
        }

        private void WriteToDatabase(DbConnection connection, List<TrafficJam> trafficJams)
        {
            if (!CreateTable(connection.CreateCommand(), tableName))
                throw new DatabaseException($"Could not create table {tableName}");
            if (!InsertRecords(connection.CreateCommand(), tableName, trafficJams))
                throw new DatabaseException("Could not insert data");
        }

        protected virtual bool InsertRecords(DbCommand command, string table, List<TrafficJam> trafficJams)
        {
            command.CommandText = $"INSERT INTO {table} (" +
                                  "    retrieval_stamp, road_name, from_lat, from_lon, to_lat, to_lon, reason, " +
                                  "    description" +
                                  ") VALUES (" +
                                  "    @retrieval_stamp, @road_name, @from_lat, @from_lon, @to_lat, " +
                                  "    @to_lon, @reason, @description" +
                                  ");";

            command.Parameters.Add(CreateParameter(command, "@retrieval_stamp", DbType.DateTime));
            command.Parameters.Add(CreateParameter(command, "@road_name", DbType.String));
            command.Parameters.Add(CreateParameter(command, "@from_lat", DbType.Decimal));
            command.Parameters.Add(CreateParameter(command, "@from_lon", DbType.Decimal));
            command.Parameters.Add(CreateParameter(command, "@to_lat", DbType.Decimal));
            command.Parameters.Add(CreateParameter(command, "@to_lon", DbType.Decimal));
            command.Parameters.Add(CreateParameter(command, "@reason", DbType.String));
            command.Parameters.Add(CreateParameter(command, "@description", DbType.String));
            command.Prepare();

            int insertedRowCount = 0;
            DateTime now = DateTime.Now.ToUniversalTime();
            foreach (TrafficJam trafficJam in trafficJams)
            {
                object[] values =
                {
                    now, trafficJam.RoadName, trafficJam.FromLocation.Latitude, trafficJam.FromLocation.Longitude,
                    trafficJam.ToLocation.Latitude, trafficJam.ToLocation.Longitude, trafficJam.Reason,
                    trafficJam.Description
                };
                for (int i = 0; i < values.Length; i++)
                    command.Parameters[i].Value = values[i];
                insertedRowCount += command.ExecuteNonQuery();
            }

            return insertedRowCount == trafficJams.Count;
        }

        protected virtual bool CreateTable(DbCommand command, string table)
        {
            string auto = GetAutoIncrementKeyword();
            command.CommandText = $"CREATE TABLE IF NOT EXISTS {table} (" +
                                  $"   id              {auto} PRIMARY KEY," +
                                  "    retrieval_stamp TIMESTAMP NOT NULL," +
                                  "    road_name       VARCHAR(255) NOT NULL," +
                                  "    from_lat        DECIMAL(8, 6) NOT NULL," +
                                  "    from_lon        DECIMAL(9, 6) NOT NULL," +
                                  "    to_lat          DECIMAL(8, 6) NOT NULL," +
                                  "    to_lon          DECIMAL(9, 6) NOT NULL," +
                                  "    reason          TEXT," +
                                  "    description     TEXT" +
                                  ");";
            command.ExecuteNonQuery();
            return true;
        }

        protected virtual string GetAutoIncrementKeyword()
        {
            return "INT AUTO_INCREMENT";
        }

        private static DbParameter CreateParameter(DbCommand command, string name, DbType type)
        {
            DbParameter parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.DbType = type;
            return parameter;
        }
    }
}
