using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace TrafficScraper.Data.Writer
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


    public class DatabaseDataWriter : DataWriter
    {
        private readonly string _connectionString;
        private readonly string _tableName;

        public DatabaseDataWriter(string connectionString, string tableName = "TrafficJam") : base()
        {
            _connectionString = connectionString;
            _tableName = tableName;
        }

        public override void WriteTrafficJams(List<TrafficJam> trafficJams)
        {
            using (OdbcConnection connection = new OdbcConnection(_connectionString))
            {
                connection.Open();
                using (OdbcTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // First create the table
                        if (!CreateTable(GetEmptyCommand(connection, transaction), _tableName))
                            throw new DatabaseException($"Could not create table {_tableName}");
                        transaction.Commit();
                        if (!InsertRecords(GetEmptyCommand(connection, transaction), _tableName, trafficJams))
                            throw new DatabaseException("Could not insert data");
                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        if (!(e is DatabaseException))
                            throw new DatabaseException("Unexpected database error", e);
                        throw;
                    }
                }
            }
        }

        protected virtual bool InsertRecords(OdbcCommand command, string table, List<TrafficJam> trafficJams)
        {
            command.CommandText = $"INSERT INTO {table} (" +
                                   "       stamp, road_name, from_lat, from_lon, to_lat, to_lon, reason, description" +
                                   ") VALUES (@timestamp, @road, @flat, @flon, @tlat, @tlon, @r, @d);";

            command.Parameters.Add(new OdbcParameter("@timestamp", OdbcType.DateTime));
            command.Parameters.Add(new OdbcParameter("@road", OdbcType.VarChar));
            command.Parameters.Add(new OdbcParameter("@flat", OdbcType.Decimal));
            command.Parameters.Add(new OdbcParameter("@flon", OdbcType.Decimal));
            command.Parameters.Add(new OdbcParameter("@tlat", OdbcType.Decimal));
            command.Parameters.Add(new OdbcParameter("@tlon", OdbcType.Decimal));
            command.Parameters.Add(new OdbcParameter("@r", OdbcType.Text));
            command.Parameters.Add(new OdbcParameter("@d", OdbcType.Text));
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

        protected virtual bool CreateTable(OdbcCommand command, string table)
        {
            command.CommandText = $"CREATE TABLE IF NOT EXISTS {table} (" +
                                   "    id           INT AUTO_INCREMENT PRIMARY KEY," +
                                   "    stamp        DATETIME NOT NULL," +
                                   "    road_name    VARCHAR(255) NOT NULL," +
                                   "    from_lat     DECIMAL(8, 6) NOT NULL," +
                                   "    from_lon     DECIMAL(9, 6) NOT NULL," +
                                   "    to_lat       DECIMAL(8, 6) NOT NULL," +
                                   "    to_lon       DECIMAL(9, 6) NOT NULL," +
                                   "    reason       TEXT," +
                                   "    description  TEXT" +
                                   ");";
            int rowsAffected = command.ExecuteNonQuery();
            Debug.WriteLine(rowsAffected);
            return true;
        }

        private static OdbcCommand GetEmptyCommand(OdbcConnection connection, OdbcTransaction transaction)
        {
            OdbcCommand command = connection.CreateCommand();
            command.Transaction = transaction;
            return command;
        }
    }
}
