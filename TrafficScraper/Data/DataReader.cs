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

        public override List<TrafficJam> FetchTrafficJams(JToken token)
        {
            JArray roadEntries = JsonUtils.EnsureType<JObject>(token).ExpectProperty<JArray>("roadEntries");

            List<TrafficJam> trafficJams = new List<TrafficJam>();
            foreach (JToken roadEntry in roadEntries)
                HandleRoadEntry(trafficJams, JsonUtils.EnsureType<JObject>(roadEntry));
            return trafficJams;
        }

        private static void HandleRoadEntry(List<TrafficJam> trafficJams, JObject obj)
        {
            string roadName = obj.ExpectStringProperty("road");
            if (!obj.ContainsKey("events")) return;
            JArray jsonTrafficJams = obj.ExpectProperty<JObject>("events").ExpectProperty<JArray>("trafficJams");
            foreach (JToken trafficJamToken in jsonTrafficJams)
            {
                JObject trafficJam = JsonUtils.EnsureType<JObject>(trafficJamToken);
                trafficJams.Add(TrafficJamFactory(roadName, trafficJam));
            }
        }

        private static TrafficJam TrafficJamFactory(string roadName, JObject obj)
        {
            return new TrafficJam
            {
                RoadName = roadName,
                FromLocation = LocationFactory(obj.ExpectProperty<JObject>("fromLoc")),
                ToLocation = LocationFactory(obj.ExpectProperty<JObject>("toLoc")),
                Reason = obj.ExpectStringProperty("reason"),
                Description = obj.ExpectStringProperty("description"),
            };
        }

        private static Location LocationFactory(JObject obj)
        {
            return new Location
            {
                Latitude = obj.ExpectDecimalProperty("lat"),
                Longitude = obj.ExpectDecimalProperty("lon"),
            };
        }

    }
}
