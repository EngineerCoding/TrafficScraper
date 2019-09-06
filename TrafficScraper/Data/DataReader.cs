using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TrafficScraper.Data
{
    public abstract class DataReader
    {
        private readonly string inputFile;

        public DataReader(string inputFile)
        {
            if (!File.Exists(inputFile))
            {
                throw new FileNotFoundException(inputFile);
            }

            this.inputFile = inputFile;
        }

        public List<TrafficJam> FetchTrafficJams()
        {
            using (FileStream fileStream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                return FetchTrafficJams(fileStream);
            }
        }

        public abstract List<TrafficJam> FetchTrafficJams(FileStream inputFileStream);
    }

    public abstract class TextDataReader : DataReader
    {
        public TextDataReader(string inputFile) : base(inputFile)
        {
        }

        public override List<TrafficJam> FetchTrafficJams(FileStream inputFileStream)
        {
            using (StreamReader streamReader = new StreamReader(inputFileStream))
            {
                return FetchTrafficJams(streamReader);
            }
        }

        public abstract List<TrafficJam> FetchTrafficJams(StreamReader inputStreamReader);
    }

    public abstract class JsonDataReader : TextDataReader
    {
        public JsonDataReader(string inputFile) : base(inputFile)
        {
        }

        public override List<TrafficJam> FetchTrafficJams(StreamReader inputStreamReader)
        {
            return FetchTrafficJams(JToken.ReadFrom(new JsonTextReader(inputStreamReader)));
        }

        public abstract List<TrafficJam> FetchTrafficJams(JToken jToken);
    }

    public class AnwbJsonDataReader : JsonDataReader
    {
        public AnwbJsonDataReader(string inputFile) : base(inputFile)
        {
        }

        public override List<TrafficJam> FetchTrafficJams(JToken jToken)
        {
            throw new NotImplementedException();
        }
    }
}
