using System;
using System.Collections.Generic;
using System.IO;
using TrafficScraper.Data;

namespace TrafficScraper
{
    public class Program
    {
        private static void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine($"{Environment.GetCommandLineArgs()[0]} [options], where [options] can be:");
            Console.WriteLine("--help, -h               Show this text");
            Console.WriteLine("--raw-dir       PATH     The directory to store downloaded files");
            Console.WriteLine("--processed-dir PATH     The directory to store processed files");
            Console.WriteLine("--fetch-url     URL      The URL to fetch data from");
            string writerOptions = string.Join(", ", typeof(WriterOption).GetEnumNames());
            string readerOptions = string.Join(", ", typeof(ReaderOption).GetEnumNames());
            Console.WriteLine($"--reader {{{readerOptions}}}");
            Console.WriteLine($"--writer {{{writerOptions}}}");
        }

        /// <summary>
        /// Entrypoint of this program which parses the following arguments from the CLI:
        /// <list type="bullet">
        /// <item><code>--raw-dir</code>: The path to output directory to put the raw downloaded files in</item>
        /// <item><code>--processed-dir</code>: The path to the directory to put the processed files in</item>
        /// <item><code>--fetch-url</code>: The url to download to use as raw data</item>
        /// </list>
        /// </summary>
        /// <param name="args">The arguments passed from the command line</param>
        public static void Main(string[] args)
        {
            List<string> arguments = new List<string>();
            arguments.AddRange(args);

            // Print the help section when requested
            string[] helpLong = FindOption(arguments, "--help", 0);
            string[] helpShort = FindOption(arguments, "-h", 0);
            if (helpLong != null || helpShort != null)
            {
                PrintHelp();
                return;
            }

            // Parse the arguments
            Options options = new Options
            {
                RawDataOutput = GetDirectory(arguments, "--raw-dir"),
                ProcessedDataOutput = GetDirectory(arguments, "--processed-dir"),
                ReaderOption = GetEnumOption<ReaderOption>(arguments, "--reader"),
                WriterOption = GetEnumOption<WriterOption>(arguments, "--writer"),
            };

            // Parse the Fetch url
            string[] urlOption = FindOption(arguments, "--fetch-url");
            if (urlOption != null)
            {
                Uri fetchUrl;
                if (!Uri.TryCreate(urlOption[0], UriKind.Absolute, out fetchUrl) ||
                    (fetchUrl.Scheme != Uri.UriSchemeHttp && fetchUrl.Scheme != Uri.UriSchemeHttps))
                {
                    Console.Error.WriteLine($"Invalid fetch url {urlOption[0]}");
                    Environment.Exit(1);
                }

                options.FetchUri = fetchUrl;
            }

            // Populate the options with defaults
            options.PopulateWithDefaults();

            // Run the program
            Run(options);
        }

        /// <summary>
        /// Method which fetches data and processes it
        /// </summary>
        /// <param name="options">The options of the program</param>
        public static void Run(Options options)
        {
            string currentDatetime = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string rawOutputFile = Path.Combine(options.RawDataOutput.FullName, currentDatetime);
            string processedOutputFile = Path.Combine(options.ProcessedDataOutput.FullName, currentDatetime);
            Downloader.DownloadToFile(options.FetchUri, rawOutputFile);

            // Process the downloaded file
            DataProcessor dataProcessor = new DataProcessor(
                options.GetDataReader(rawOutputFile), options.GetDataWriter(processedOutputFile));
            dataProcessor.Process();
        }

        /// <summary>
        /// Finds a directory argument, and attempts to create the directory. When the directory could not be
        /// created, this writes an error to the CLI and exits the program.
        /// </summary>
        /// <param name="arguments">The arguments from the CLI</param>
        /// <param name="optionName">The name of the option which takes a directory argument</param>
        /// <returns>The info of the directory to use</returns>
        private static DirectoryInfo GetDirectory(List<string> arguments, string optionName)
        {
            string[] foundOptions = FindOption(arguments, optionName);
            if (foundOptions == null) return null;

            string directory = foundOptions[0];
            if (!Directory.Exists(directory))
            {
                try
                {
                    return Directory.CreateDirectory(directory);
                }
                catch (UnauthorizedAccessException)
                {
                    Console.Error.WriteLine($"Could not create directory {directory}");
                    Environment.Exit(1);
                }
            }

            return new DirectoryInfo(directory);
        }

        private static T GetEnumOption<T>(List<string> args, string optionName)
        {
            string[] foundOptions = FindOption(args, optionName);
            string option = foundOptions?[0];
            // Could not find the option
            if (option == null) return default(T);
            // Assuming the type is actually enum, otherwise this raises an exception
            Type enumType = typeof(T);
            string[] enumNames = enumType.GetEnumNames();
            // Find the name with the correct capitalization and stuff (ignoring case when comparing)
            foreach (string enumName in enumNames)
            {
                if (enumName.Equals(option, StringComparison.InvariantCultureIgnoreCase))
                    // Found the value, return the actual enum value
                    return (T) Enum.Parse(enumType, enumName);
            }

            // Couldn't find the enum, list the choices
            Console.Error.WriteLine($"The value {option} is not valid for {optionName}");
            Console.Error.WriteLine("Use one of the following values (case-insensitive):");
            foreach (string enumName in enumNames)
                Console.Error.WriteLine(enumName);
            Environment.Exit(1);
            // Never hits, but the compiler otherwise complains
            return default(T);
        }

        /// <summary>
        /// Fetches the amount of options from the command line after the option name. When the amount of options
        /// could not be found, this will write an error to the console screen and exit the program. Note that this
        /// method returns null when the option could not be found in the CLI arguments.
        /// </summary>
        /// <param name="arguments">The arguments from the CLI</param>
        /// <param name="optionName">The name of the option as it appears on the CLI</param>
        /// <param name="options">The amount of values this option uses</param>
        /// <returns>An array containing the values of the options, or null when the option could not be found</returns>
        private static string[] FindOption(List<string> arguments, string optionName, int options = 1)
        {
            int optionIndex = arguments.IndexOf(optionName);
            if (optionIndex != -1)
            {
                int foundItemsCounter = 0;
                string[] foundOptions = new string[options];
                for (int index = optionIndex + 1; index < arguments.Count; index++)
                {
                    foundOptions[foundItemsCounter++] = arguments[index];
                }

                // Check if we actually found all the required options
                if (foundItemsCounter != options)
                {
                    Console.Error.WriteLine($"Expected {options} for option {optionName}," +
                                            $" actually found {foundOptions} options.");
                    Environment.Exit(1);
                }

                // Remove the options from the original arguments
                // Because when removing items the list shifts, we are removing the same index multiple times
                for (int i = 0; i < options + 1; i++)
                    arguments.RemoveAt(optionIndex);
                // Return the found options
                return foundOptions;
            }

            return null;
        }
    }

    public enum ReaderOption : byte
    {
        Default,
        AnwbJsonReader
    }

    public enum WriterOption : byte
    {
        Default,
        CsvWriter
    }

    public class Options
    {
        private const string DefaultRawOutput = "raw";
        private const string DefaultProcessedOutput = "processed";
        private const string DefaultFetchUri = "https://www.anwb.nl/feeds/gethf";

        public DirectoryInfo RawDataOutput { get; set; }
        public DirectoryInfo ProcessedDataOutput { get; set; }
        public Uri FetchUri { get; set; }

        public ReaderOption ReaderOption { get; set; } = ReaderOption.Default;

        public WriterOption WriterOption { get; set; } = WriterOption.Default;

        /// <summary>
        /// Sets the properties of this object to their respective defaults if they have not been set yet
        /// </summary>
        public void PopulateWithDefaults()
        {
            if (RawDataOutput == null)
                RawDataOutput = CreateOrGetDirInfo(DefaultRawOutput);

            if (ProcessedDataOutput == null)
                ProcessedDataOutput = CreateOrGetDirInfo(DefaultProcessedOutput);

            if (FetchUri == null)
                FetchUri = new Uri(DefaultFetchUri);
        }

        public DataReader GetDataReader(string inputFile)
        {
            switch (ReaderOption)
            {
                case ReaderOption.Default:
                case ReaderOption.AnwbJsonReader:
                    return new AnwbJsonDataReader(inputFile);
            }

            return null;
        }

        public DataWriter GetDataWriter(string outputFile)
        {
            switch (WriterOption)
            {
                case WriterOption.Default:
                case WriterOption.CsvWriter:
                    return new CsvDataWriter(outputFile);
            }

            return null;
        }

        /// <summary>
        /// Fetches the DirectoryInfo of the path by either creating the directory if it does not exist yet or simply
        /// creating the DirectoryInfo object.
        /// </summary>
        /// <param name="path">The path to the directory</param>
        /// <returns>The associated DirectoryInfo of the path</returns>
        private static DirectoryInfo CreateOrGetDirInfo(string path)
        {
            return Directory.Exists(path) ? new DirectoryInfo(path) : Directory.CreateDirectory(path);
        }
    }
}
