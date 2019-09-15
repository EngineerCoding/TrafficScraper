using System.Collections.Generic;

namespace TrafficScraper.Data.Writer
{
    public abstract class DataWriter
    {
        public bool IncludeReason { get; set; } = true;
        public bool IncludeDescription { get; set; } = true;

        public abstract void WriteTrafficJams(List<TrafficJam> trafficJams);
    }
}
