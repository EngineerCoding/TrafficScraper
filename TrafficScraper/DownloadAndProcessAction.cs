using System;
using System.IO;
using Scheduler.Delegate;
using TrafficScraper.Data;

namespace TrafficScraper
{
    public class DownloadAndProcessAction : BaseAction
    {
        private readonly Options _options;

        public DownloadAndProcessAction(Options options)
        {
            _options = options;
            options.PopulateWithDefaults();
        }

        public override void Execute()
        {
            string currentDatetime = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string rawOutputFile = Path.Combine(_options.RawDataOutput.FullName, currentDatetime);
            Downloader.DownloadToFile(_options.FetchUri, rawOutputFile);

            // Process the downloaded file
            DataProcessor dataProcessor = new DataProcessor(
                _options.GetDataReader(rawOutputFile), _options.GetDataWriter());
            dataProcessor.Process();

            if (_options.RemoveRawFiles)
            {
                File.Delete(rawOutputFile);
            }
        }
    }
}
