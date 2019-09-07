using System;
using System.Threading;
using Scheduler.Delegate;
using Scheduler.Interval;

namespace Scheduler
{
    class Program
    {
        static void Main(string[] args)
        {
            // Check if we need to print help by checking for -h or --help at the first or second index
            // Also, when fewer arguments are available, print the help
            int shortIndex = Array.IndexOf(args, "-h");
            int longIndex = Array.IndexOf(args, "--help");
            if (args.Length < 2 || shortIndex == 0 || shortIndex == 1 || longIndex == 0 || longIndex == 1)
            {
                PrintHelp();
                return;
            }

            string commandFileName = args[1];
            string commandArguments = CommandAction.BuildArguments(args, 2);
            IntervalRange[] intervals = GetIntervals(args[0]);

            IntervalScheduler intervalScheduler = new IntervalScheduler(
                new CommandAction(commandFileName, commandArguments), intervals);
            intervalScheduler.RunScheduler(CancellationToken.None);
        }

        private static IntervalRange[] GetIntervals(string suppliedIntervalString)
        {
            string[] definedIntervals = suppliedIntervalString.Split(",");
            IntervalRange[] intervals = new IntervalRange[definedIntervals.Length];

            int index = 0;
            foreach (string definedInterval in definedIntervals)
            {
                IntervalRange parsedInterval = IntervalRange.ParseInterval(definedInterval);
                if (parsedInterval == null)
                    Console.Error.Write("Invalid interval string: " + definedInterval);
                else
                    intervals[index++] = parsedInterval;
            }

            // Check if we actually have all intervals
            if (index != intervals.Length)
                Environment.Exit(1);
            return intervals;
        }

        public static void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine($"{Environment.GetCommandLineArgs()[0]} [interval,interval2,..] [command]");
            Console.WriteLine("Where [interval] is a string in the format dd:dd-dd:dd-d+[hm], for example when " +
                              "something has to be run between 9 AM and 5 PM every 10 minutes the following stri" +
                              "ng is used: 09:00-17:00-10m. When multiple intervals have to be defined, these ca" +
                              "n be added by delimiting the intervals with a comma.");
            Console.WriteLine("After the first arguments, the intervals (which is one long string!), the command" +
                              " to execute is added.");
        }
    }
}
