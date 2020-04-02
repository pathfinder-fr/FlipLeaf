using System;

namespace FlipLeaf
{
    public static class Extensions
    {
        public static bool EqualsOrdinal(this string? @this, string? other)
        {
            if (@this == null && other == null) return true;
            if (@this == null || other == null) return false;

            return string.Equals(@this, other, StringComparison.Ordinal);
        }

        public static string ToRelativeTime(this DateTimeOffset? @this)
        {
            if (@this == null)
                return string.Empty;

            return ToRelativeTime(@this.Value);
        }

        public static string ToRelativeTime(this DateTimeOffset @this)
        {
            var ago = DateTimeOffset.Now - @this;

            if (ago.TotalMinutes < 1)
                return "Just now";
            if (ago.TotalMinutes < 2)
                return "1 minute ago";
            if (ago.TotalMinutes < 60)
                return $"{ago.TotalMinutes:0} minutes ago";
            if (ago.TotalHours < 2)
                return "1 hour ago";
            if (ago.TotalHours < 24)
                return $"{ago.TotalHours:0} hours ago";
            if (ago.TotalDays < 2)
                return "1 day ago";
            if (ago.TotalDays < 7)
                return $"{ago.TotalDays:0} days ago";
            if (ago.TotalDays / 7 == 1)
                return $"1 week ago";
            if (ago.TotalDays / 7 == 4)
                return $"{(ago.TotalDays / 7):0} weeks ago";

            return $"{ago.TotalDays / 30:0} months ago";
        }
    }
}
