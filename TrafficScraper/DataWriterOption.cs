using System;
using System.Collections.Generic;
using System.Reflection;
using TrafficScraper.Data.Writer;

namespace TrafficScraper
{
    public abstract class DataWriterOption
    {
        public abstract bool isAvailable(List<string> args);

        public abstract bool validArguments();

        public abstract DataWriter GetWriter();
    }


    public class DbDataWriterOption : DataWriterOption
    {
        private const string ODBC_OPTION = "TrafficScraper.Data.Writer.Database.OdbcDataWriter";
        private const string PSQL_OPTION = "TrafficScraper.Data.Writer.Database.PostgreSqlDataWriter";

        private string[] _arguments = null;
        private Type type;

        public override bool isAvailable(List<string> args)
        {
            // Check if we are using ODBC
            if (IsSubOptionAvailable(args, "--odbc"))
            {
                type = Assembly.GetExecutingAssembly().GetType(ODBC_OPTION);
            }
            else if (IsSubOptionAvailable(args, "--psql"))
            {
                type = Assembly.GetExecutingAssembly().GetType(PSQL_OPTION);
            }
            else
            {
                return false;
            }

            return true;
        }

        private bool IsSubOptionAvailable(List<string> args, string option)
        {
            _arguments = Program.FindOption(args, option, 2, false);
            if (_arguments == null)
                _arguments = Program.FindOption(args, option);

            return _arguments != null;
        }

        public override bool validArguments()
        {
            if (_arguments == null)
                throw new InvalidOperationException();
            // Cannot really check the arguments
            return _arguments != null;
        }

        public override DataWriter GetWriter()
        {
            if (_arguments == null)
                throw new InvalidOperationException();

            return (DataWriter) Activator.CreateInstance(type, _arguments);
        }
    }


    public class FileDataWriterOption : DataWriterOption
    {
        private string _outputFile = null;

        public override bool isAvailable(List<string> args)
        {
            string[] options = Program.FindOption(args, "--output-file", 1, false);
            if (options != null)
            {
                _outputFile = options[0];
            }
            else if (Program.FindOption(args, "--output-file", 0) != null)
            {
                _outputFile = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            }
            else
            {
                return false;
            }

            return true;
        }

        public override bool validArguments()
        {
            return _outputFile != null;
        }

        public override DataWriter GetWriter()
        {
            return new CsvFileDataWriter(_outputFile);
        }
    }
}
