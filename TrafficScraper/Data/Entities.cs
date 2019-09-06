using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace TrafficScraper.Data
{
    public class Location
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public string LatitudeToString()
        {
            return Latitude.ToString(CultureInfo.InvariantCulture);
        }

        public string LongitudeToString()
        {
            return Longitude.ToString(CultureInfo.InvariantCulture);
        }
    }

    public class TrafficJam
    {
        public string RoadName { get; set; }
        public Location FromLocation { get; set; }
        public Location ToLocation { get; set; }

        public string Reason { get; set; }
        public string Description { get; set; }

        public void Validate()
        {
            string roadName = RoadName?.Trim();
            if (string.IsNullOrEmpty(roadName))
                throw new InvalidTrafficJamException("Road must be set!");
            ValidateLocation(FromLocation, "FromLocation");
            ValidateLocation(ToLocation, "ToLocation");

            if (Reason == null) Reason = String.Empty;
            if (Description == null) Description = String.Empty;
        }

        private static void ValidateLocation(Location location, string field)
        {
            if (location == null)
                throw new InvalidTrafficJamException($"{field} must be set!");
        }
    }

    [Serializable]
    public class InvalidTrafficJamException : Exception
    {
        public InvalidTrafficJamException(string message) : base(message)
        {
        }

        public InvalidTrafficJamException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public InvalidTrafficJamException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
