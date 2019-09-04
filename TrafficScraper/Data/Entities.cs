namespace TrafficScraper.Data
{
    public class Location
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }
    
    public class TrafficJam
    {
        public string RoadName { get; set; }
        public Location FromLocation { get; set; }
        public Location ToLocation { get; set; }
        
        public string Reason { get; set; }
        public string Description { get; set; }
    }
}
