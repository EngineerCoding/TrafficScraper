using System;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;

namespace TrafficScraper.Data
{
    public static class JsonUtils
    {
        private static void CheckExists(JObject obj, string property)
        {
            if (!obj.ContainsKey(property))
                throw new MissingFieldException(property);
        }

        public static string ExpectStringProperty(this JObject obj, string property)
        {
            CheckExists(obj, property);
            return (string) obj[property];
        }

        public static decimal ExpectDecimalProperty(this JObject obj, string property)
        {
            CheckExists(obj, property);
            return (decimal) obj[property];
        }

        public static T ExpectProperty<T>(this JObject obj, string property) where T : JToken
        {
            CheckExists(obj, property);
            return EnsureType<T>(obj[property]);
        }

        public static T EnsureType<T>(JToken value) where T : JToken
        {
            if (!(value is T))
                throw new UnexpectedJsonValue(typeof(T), value.GetType());
            return (T) value;
        }
    }

    [Serializable]
    public class UnexpectedJsonValue : Exception
    {
        public UnexpectedJsonValue(Type expected, Type received) : base(GetMessage(expected, received))
        {
        }

        public UnexpectedJsonValue(Type expected, Type received, Exception inner) : base(
            GetMessage(expected, received), inner)
        {
        }

        public UnexpectedJsonValue(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        private static string GetMessage(Type expected, Type received)
        {
            return $"Expected {expected.FullName}, but received {received.FullName}";
        }
    }
}
