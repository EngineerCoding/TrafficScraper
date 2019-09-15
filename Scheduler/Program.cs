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

            int commandIndex = 1;
            byte runOnDays = 0;
            if (args[1] == "--days")
            {
                commandIndex += 2;
                string[] splitDays = args[2].Split(",");

                for (int i = 0; i < splitDays.Length; i++)
                {
                    if (splitDays[i].Length == 1)
                    {
                        // Attempt to parse it as an integer
                        int dayIndex;
                        if (int.TryParse(splitDays[i], out dayIndex))
                        {
                            Day? foundIntDay = DayUtils.FindDay(dayIndex);
                            if (foundIntDay.HasValue)
                            {
                                runOnDays |= (byte) foundIntDay.Value;
                                continue;
                            }
                        }
                    }
                    // Attempt to find the day as valid string
                    Day? foundStrDay = DayUtils.FindDay(splitDays[i]);
                    if (!foundStrDay.HasValue)
                    {
                        Console.Error.WriteLine($"Invalid day: {splitDays[i]}");
                        Environment.Exit(1);
                    }
                    runOnDays |= (byte) foundStrDay.Value;
                }
            }

            string commandFileName = args[commandIndex++];
            string commandArguments = CommandAction.BuildArguments(args, commandIndex);
            IntervalRange[] intervals = IntervalRange.GetIntervals(args[0], Console.Error);
            if (intervals == null) Environment.Exit(1);

            IntervalScheduler intervalScheduler = new IntervalScheduler(
                new CommandAction(commandFileName, commandArguments), intervals);
            if (runOnDays != 0)
                intervalScheduler.SetDayFlags(runOnDays);

            intervalScheduler.RunScheduler(CancellationToken.None);
        }

        public static void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine($"{Environment.GetCommandLineArgs()[0]} [interval,interval2,..] [--days 0,1,2,3]" +
                               " [command]");
            Console.WriteLine("Where [interval] is a string in the format dd:dd-dd:dd-d+[hm], for example when " +
                              "something has to be run between 9 AM and 5 PM every 10 minutes the following stri" +
                              "ng is used: 09:00-17:00-10m. When multiple intervals have to be defined, these ca" +
                              "n be added by delimiting the intervals with a comma.");
            Console.WriteLine("After the first arguments, the intervals (which is one long string!), the command" +
                              " to execute is added.");
        }
    }
}
