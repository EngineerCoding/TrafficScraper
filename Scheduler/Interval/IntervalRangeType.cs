using System;

namespace Scheduler.Interval
{
    public enum IntervalRangeType
    {
        Minutes = 1,
        Hours = 60,
    }

    public static class IntervalRangeTypeUtil
    {
        public static IntervalRangeType GetFromChar(char intervalTypeLetter)
        {
            switch (char.ToLower(intervalTypeLetter))
            {
                case 'm': return IntervalRangeType.Minutes;
                case 'h': return IntervalRangeType.Hours;
            }

            // Should technically never reach this, as we already validated through a regex
            throw new NotImplementedException();
        }
    }
}
