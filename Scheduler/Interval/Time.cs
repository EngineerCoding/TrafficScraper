using System;

namespace Scheduler.Interval
{
    public class Time
    {
        public const int MaxHour = 23;
        public const int MaxMinute = 60;

        private byte _hour;
        private byte _minute;

        public byte Hour
        {
            set
            {
                CheckRange(value, MaxHour);
                _hour = value;
            }
            get => _hour;
        }

        public byte Minute
        {
            set
            {
                CheckRange(value, MaxMinute);
                _minute = value;
            }
            get => _minute;
        }

        internal short ToShort()
        {
            int minutes = Hour * MaxMinute + Minute;
            return (short) minutes;
        }

        private static void CheckRange(int value, int maximum)
        {
            if (value < 0 || value > maximum)
                throw new ArgumentOutOfRangeException($"${value} should be in the range [0, {maximum}]");
        }

        public static bool operator ==(Time left, Time right)
        {
            return left.ToShort() == right.ToShort();
        }

        public static bool operator !=(Time left, Time right)
        {
            return left.ToShort() != right.ToShort();
        }

        public static bool operator <(Time left, Time right)
        {
            return left.ToShort() < right.ToShort();
        }

        public static bool operator <=(Time left, Time right)
        {
            return left.ToShort() <= right.ToShort();
        }

        public static bool operator >(Time left, Time right)
        {
            return left.ToShort() > right.ToShort();
        }

        public static bool operator >=(Time left, Time right)
        {
            return left.ToShort() >= right.ToShort();
        }

        public override bool Equals(object obj)
        {
            Time other = obj as Time;
            if (other == null) return false;

            return this == other;
        }

        public override int GetHashCode()
        {
            return 17 * ToShort();
        }
    }

    public static class DateTimeExtension
    {
        public static DateTime ForwardToTime(this DateTime dateTime, Time time)
        {
            // If the time already passed, increment to the next day
            if (dateTime.GetTime().ToShort() > time.ToShort())
                dateTime = dateTime.AddDays(1);
            return new DateTime(
                dateTime.Year, dateTime.Month, dateTime.Day, time.Hour, time.Minute, 0, 0);
        }

        public static DateTime AddMinutesRound(this DateTime dateTime, int minutes)
        {
            dateTime = dateTime.AddMinutes(minutes);
            return dateTime.RemoveSecondAndMillisecond();
        }

        public static DateTime RemoveSecondAndMillisecond(this DateTime dateTime)
        {
            return new DateTime(
                dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0, 0);
        }

        public static Time GetTime(this DateTime dateTime)
        {
            return new Time
            {
                Hour = (byte) dateTime.Hour, Minute = (byte) dateTime.Minute
            };
        }
    }
}
