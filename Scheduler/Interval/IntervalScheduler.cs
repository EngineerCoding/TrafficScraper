using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Scheduler.Delegate;

namespace Scheduler.Interval
{
    public class IntervalScheduler : Scheduler
    {
        private readonly IntervalRange[] _intervals;

        public IntervalScheduler(Action action, IntervalRange range) : this(action, new[] {range})
        {
        }

        public IntervalScheduler(Action action, IntervalRange[] ranges) : base(action)
        {
            _intervals = ranges;
            CheckIntervals();
        }

        public IntervalScheduler(BaseAction action, IntervalRange range) : this(action, new[] {range})
        {
        }

        public IntervalScheduler(BaseAction action, IntervalRange[] ranges) : base(action)
        {
            _intervals = ranges;
            CheckIntervals();
        }

        private void CheckIntervals()
        {
            if (_intervals.Any(interval => interval == null))
            {
                throw new NullReferenceException("Null values have been found in the intervals array");
            }
        }

        public override void RunScheduler(CancellationToken cancellationToken)
        {
            DateTime[] executeOnDateTime = new DateTime[_intervals.Length];
            // Fill the array with the next datetime of each interval
            DateTime previousDateTime = DateTime.Now;
            for (int i = 0; i < _intervals.Length; i++)
                executeOnDateTime[i] = _intervals[i].GetNextDateTime(previousDateTime);

            while (true)
            {
                int nearestIndex = FindNearestIndex(executeOnDateTime);
                SleepUntilDateTime(executeOnDateTime[nearestIndex], cancellationToken);
                // Check if we were cancelled
                cancellationToken.ThrowIfCancellationRequested();
                // First increase the time to calculate the next datetime object
                DateTime forDateTime = DateTime.Now.AddMinutesRound(1);
                if (forDateTime == previousDateTime)
                    forDateTime = forDateTime.AddMinutes(1);
                previousDateTime = forDateTime;
                // Execute
                RunActionAsync();
                // Get the next DateTime
                executeOnDateTime[nearestIndex] = _intervals[nearestIndex].GetNextDateTime(forDateTime);
            }
        }

        // statics

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0);

        private static void SleepUntilDateTime(DateTime untilDateTime, CancellationToken? cancellationToken = null)
        {
            TimeSpan sleepTime = untilDateTime - DateTime.Now;
            Console.WriteLine(sleepTime);
            if (sleepTime.TotalSeconds > 0D)
            {
                if (cancellationToken.HasValue)
                {
                    CancellationToken cancelToken = cancellationToken.Value;
                    Task.Delay(sleepTime, cancelToken).Wait(cancelToken);
                }
                else
                {
                    // Doesn't require cancellation
                    Thread.Sleep(sleepTime);
                }
            }
        }

        private static int FindNearestIndex(DateTime[] dateTimes)
        {
            long minValue = long.MaxValue;
            int minIndex = 0;
            for (int i = 0; i < dateTimes.Length; i++)
            {
                long timestamp = (long) (dateTimes[i] - Epoch).TotalSeconds;
                if (timestamp < minValue)
                {
                    minValue = timestamp;
                    minIndex = i;
                }
            }

            return minIndex;
        }
    }
}
