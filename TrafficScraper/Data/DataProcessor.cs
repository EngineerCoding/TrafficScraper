using System.Collections.Generic;

namespace TrafficScraper.Data
{
    class DataProcessor
    {
        private readonly DataReader reader;
        private readonly DataWriter writer;
        private bool processed = false;

        public DataProcessor(DataReader reader, DataWriter writer)
        {
            this.reader = reader;
            this.writer = writer;
        }

        public void Process()
        {
            if (processed) return;
            processed = true;

            List<TrafficJam> trafficJams = reader.FetchTrafficJams();
            writer.WriteTrafficJams(trafficJams);
        }
    }
}
