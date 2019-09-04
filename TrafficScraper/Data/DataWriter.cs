using System.Collections.Generic;
using System.IO;

namespace TrafficScraper.Data
{
    public abstract class DataWriter
    {
        private readonly string outputFile; 

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
    }

    public abstract class TextDataWriter : DataWriter
    {
        public TextDataWriter(string outputFile) : base(outputFile) { }

        public override void WriteTrafficJams(FileStream outputFileStream, List<TrafficJam> trafficJams)
        {
            using (StreamWriter streamWriter = new StreamWriter(outputFileStream))
            {
                WriteTrafficJams(streamWriter, trafficJams);
            }
        }

        public abstract void WriteTrafficJams(StreamWriter streamWriter, List<TrafficJam> trafficJams);
    }
}
