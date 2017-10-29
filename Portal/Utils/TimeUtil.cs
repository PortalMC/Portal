using System;

namespace Portal.Utils
{
    public static class TimeUtil
    {
        private const int Second = 1;
        private const int Minute = 60 * Second;
        private const int Hour = 60 * Minute;
        private const int Day = 24 * Hour;

        public static string GetRelativeTimeLabel(TimeSpan ts)
        {
            if (ts.TotalMinutes < 1)
                return "just now";

            if (ts.TotalMinutes < 2)
                return "a minute ago";

            if (ts.TotalMinutes < 60)
                return ts.Minutes + " minutes ago";

            if (ts.TotalHours <= 1)
                return "an hour ago";

            if (ts.TotalDays < 1)
                return ts.Hours + " hours ago";

            if (ts.TotalDays < 2)
                return "yesterday";
            return ts.Days + " days ago";
        }
    }
}