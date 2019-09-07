using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Scheduler.Interval
{
    public class IntervalRange
    {
        private readonly Time _startingTime;
        private readonly Time _endingTime;
        private readonly long _timeout;

        public IntervalRange(Time startingTime, Time endingTime, long timeout,
            IntervalRangeType intervalRangeType = IntervalRangeType.Minutes)
        {
            _startingTime = startingTime;
            _endingTime = endingTime;
            // Validate that the ending time is actually bigger than the startingTime
            if (endingTime <= startingTime)
                throw new InvalidOperationException("Ending time is not larger the starting time");

            // Raise an error when we overflow
            checked
            {
                _timeout = timeout * (int) intervalRangeType;
            }
        }

        public long GetMinuteTimeout()
        {
            return _timeout;
        }

        public DateTime GetNextDateTime(DateTime fromMoment)
        {
            Time fromMomentTime = fromMoment.GetTime();
            short fromTime = fromMomentTime.ToShort();
            // Check if we have to apply the initial run
            if (_startingTime.ToShort() >= fromTime || _endingTime.ToShort() <= fromTime)
                return fromMoment.ForwardToTime(_startingTime);
            // Calculate what the closest interval point is
            // First calculate the difference in minutes
            int diffMinutes = (fromMomentTime.Hour - _startingTime.Hour) * Time.MaxMinute +
                              fromMomentTime.Minute - _startingTime.Minute;
            // Calculate how many times the timeout has passed in this difference
            float timeoutTimes = (float) diffMinutes / GetMinuteTimeout();
            // Check if this is approximately a round number
            // Because then we have to undertake action now!
            if (timeoutTimes % 1 < double.Epsilon) return fromMoment;

            DateTime nextDateTime = fromMoment.AddMinutesRound((int) (Math.Ceiling(timeoutTimes) * GetMinuteTimeout()));
            // Make sure the time does not exceed the end time
            if (nextDateTime.GetTime() > _endingTime)
                return fromMoment.ForwardToTime(_startingTime);
            return nextDateTime;
        }

        private static Regex IntervalRegex = new Regex(
            @"^(\d{2}):(\d{2})-(\d{2}):(\d{2})-(\d+)([hmHM])$", RegexOptions.Compiled);

        public static IntervalRange ParseInterval(string interval)
        {
            Match match = IntervalRegex.Match(interval);
            if (!match.Success) return null;

            // Create the time objects
            Time startingTime, endingTime;
            try
            {
                startingTime = new Time
                {
                    Hour = byte.Parse(match.Groups[1].Value), Minute = byte.Parse(match.Groups[2].Value)
                };
                endingTime = new Time
                {
                    Hour = byte.Parse(match.Groups[3].Value), Minute = byte.Parse(match.Groups[4].Value)
                };
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }

            // Get the timeout and interval type (which don't require additional validation)
            long timeout = long.Parse(match.Groups[5].Value);
            IntervalRangeType intervalRangeType = IntervalRangeTypeUtil.GetFromChar(match.Groups[6].Value[0]);

            return new IntervalRange(startingTime, endingTime, timeout, intervalRangeType);
        }

        public static IntervalRange[] GetIntervals(string suppliedIntervalString, TextWriter console = null)
        {
            string[] definedIntervals = suppliedIntervalString.Split(",");
            IntervalRange[] intervals = new IntervalRange[definedIntervals.Length];

            int index = 0;
            foreach (string definedInterval in definedIntervals)
            {
                IntervalRange parsedInterval = ParseInterval(definedInterval);
                if (parsedInterval == null)
                    console?.WriteLine("Invalid interval string: " + definedInterval);
                else
                    intervals[index++] = parsedInterval;
            }

            // Check if we actually have all intervals
            if (index != intervals.Length)
                return null;
            return intervals;
        }
    }
}
