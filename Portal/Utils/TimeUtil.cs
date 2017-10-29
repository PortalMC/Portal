using System;

namespace Portal.Utils
{
    public static class TimeUtil
    {
        public static string GetRelativeTimeLabel(TimeSpan ts)
        {
            if (ts.TotalSeconds < 2)
                return "a second ago";

            if (ts.TotalSeconds < 60)
                return ts.Seconds + " seconds ago";

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