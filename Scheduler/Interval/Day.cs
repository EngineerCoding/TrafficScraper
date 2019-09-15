using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Scheduler.Interval
{
    public enum Day : byte
    {
        Monday = 1,
        Tuesday = 2,
        Wednesday = 4,
        Thursday = 8,
        Friday = 16,
        Saturday = 32,
        Sunday = 64,
    }

    public static class DayUtils
    {
        private static readonly string[] DayNames;
        private static readonly Day[] EnumValues;

        static DayUtils()
        {
            Type dayType = typeof(Day);
            DayNames = dayType.GetEnumNames();
            EnumValues = new Day[DayNames.Length];
            for (int i = 0; i < DayNames.Length; i++)
            {
                EnumValues[i] = (Day) Enum.Parse(dayType, DayNames[i]);
                DayNames[i] = DayNames[i].ToLower();
            }
        }

        public static byte GetMaxValue()
        {
            byte value = 0;
            foreach (Day day in EnumValues)
                value |= (byte) day;
            return value;
        }

        public static byte GetFlag(this DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Monday: return (byte) Day.Monday;
                case DayOfWeek.Tuesday: return (byte) Day.Tuesday;
                case DayOfWeek.Wednesday: return (byte) Day.Wednesday;
                case DayOfWeek.Thursday: return (byte) Day.Thursday;
                case DayOfWeek.Friday: return (byte) Day.Friday;
                case DayOfWeek.Saturday: return (byte) Day.Saturday;
                case DayOfWeek.Sunday: return (byte) Day.Sunday;
                default: return 0; // should never hit
            }
        }

        public static Day? FindDay(string day)
        {
            day = day.ToLower();
            for (int i = 0; i < DayNames.Length; i++)
            {
                if (DayNames[i].Equals(day) || DayNames[i].StartsWith(day))
                    return EnumValues[i];
            }

            return null;
        }

        public static Day? FindDay(int day)
        {
            if (day < 0 || day >= EnumValues.Length)
                return null;
            return EnumValues[day];
        }
    }
}
