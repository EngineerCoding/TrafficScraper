using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization;

namespace TrafficScraper
{
    [Serializable]
    public class FileExistsException : Exception
    {
        public FileExistsException(string path) : base(GetMessage(path))
        {
        }

        public FileExistsException(string path, Exception innerException) : base(GetMessage(path), innerException)
        {
        }

        public FileExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        private static string GetMessage(string path)
        {
            return $"File {path} already exists!";
        }
    }


    public class Downloader
    {
        public static string DownloadUrl(string url)
        {
            return DownloadUrl(new Uri(url));
        }

        public static string DownloadUrl(Uri url)
        {
            using (WebClient client = new WebClient())
            {
                return client.DownloadString(url);
            }
        }

        public static void DownloadToFile(string url, string path)
        {
            DownloadToFile(new Uri(url), path);
        }

        public static void DownloadToFile(Uri url, string path)
        {
            if (File.Exists(path))
            {
                throw new FileExistsException(path);
            }

            using (WebClient client = new WebClient())
            {
                client.DownloadFile(url, path);
            }
        }

        public static HttpWebResponse GetWebResponse(string url)
        {
            return GetWebResponse(new Uri(url));
        }

        public static HttpWebResponse GetWebResponse(Uri url)
        {
            HttpWebRequest webRequest = (HttpWebRequest) WebRequest.Create(url);
            return (HttpWebResponse) webRequest.GetResponse();
        }

        public static Stream DownloadUrlAsStream(string url)
        {
            return DownloadUrlAsStream(new Uri(url));
        }

        public static Stream DownloadUrlAsStream(Uri url)
        {
            return GetWebResponse(url).GetResponseStream();
        }
    }
}
