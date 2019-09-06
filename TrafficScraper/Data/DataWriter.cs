using System.Collections.Generic;
using System.IO;

namespace TrafficScraper.Data
{
    public abstract class DataWriter
    {
        private readonly string outputFile;

        public bool IncludeReason { get; set; } = true;
        public bool IncludeDescription { get; set; } = true;

        public DataWriter(string outputFile)
        {
            this.outputFile = outputFile;
        }

        public void WriteTrafficJams(List<TrafficJam> trafficJams)
        {
            using (FileStream fileStream = File.Open(outputFile, FileMode.Create, FileAccess.Write))
            {
                WriteTrafficJams(fileStream, trafficJams);
            }
        }

        public abstract void WriteTrafficJams(FileStream outputFileStream, List<TrafficJam> trafficJams);

        public string[] GetTrafficJamValues(TrafficJam trafficJam)
        {
            string[] values = new string[5 + (IncludeReason ? 1 : 0) + (IncludeDescription ? 1 : 0)];
            int index = -1;
            values[++index] = trafficJam.RoadName;
            values[++index] = trafficJam.FromLocation.LatitudeToString();
            values[++index] = trafficJam.FromLocation.LongitudeToString();
            values[++index] = trafficJam.ToLocation.LatitudeToString();
            values[++index] = trafficJam.ToLocation.LongitudeToString();

            if (IncludeReason)
                values[++index] = trafficJam.Reason;
            if (IncludeDescription)
                values[++index] = trafficJam.Description;
            return values;
        }
    }

    public abstract class TextDataWriter : DataWriter
    {
        public TextDataWriter(string outputFile) : base(outputFile)
        {
        }

        public override void WriteTrafficJams(FileStream outputFileStream, List<TrafficJam> trafficJams)
        {
            using (StreamWriter streamWriter = new StreamWriter(outputFileStream))
            {
                WriteTrafficJams(streamWriter, trafficJams);
            }
        }

        public abstract void WriteTrafficJams(StreamWriter streamWriter, List<TrafficJam> trafficJams);
    }

    public class CsvDataWriter : TextDataWriter
    {
        public char Delimiter { get; set; } = ',';

        public CsvDataWriter(string outputFile) : base(outputFile)
        {
        }

        public override void WriteTrafficJams(StreamWriter streamWriter, List<TrafficJam> trafficJams)
        {
            string header = "road,from_lat,from_lon,to_lat,to_lon";
            if (IncludeReason) header += ",reason";
            if (IncludeDescription) header += ",description";

            streamWriter.WriteLine(header);
            trafficJams.ForEach(trafficJam => streamWriter.WriteLine(FormatTrafficJam(trafficJam)));
        }

        /// <summary>
        /// Format a TrafficJam object to a string delimited by the specified delimiter on this object
        /// </summary>
        /// <param name="trafficJam">The object to format</param>
        /// <returns>The delimiter joined data</returns>
        private string FormatTrafficJam(TrafficJam trafficJam)
        {
            string delimiter = $"\"{Delimiter}\"";

            return "\"" + string.Join(delimiter, GetTrafficJamValues(trafficJam)) + "\"";
        }
    }
}
