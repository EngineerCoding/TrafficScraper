using System;
using System.Collections.Generic;
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
        private string[] _arguments = null;

        public override bool isAvailable(List<string> args)
        {
            _arguments = Program.FindOption(args, "--database-output", 2, false);
            if (_arguments == null)
                _arguments = Program.FindOption(args, "--database-output");

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

            switch (_arguments.Length)
            {
                case 1: return new DatabaseDataWriter(_arguments[0]);
                case 2: return new DatabaseDataWriter(_arguments[0], _arguments[1]);
            }

            return null;
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
