using System;

namespace DragonLib
{
    // Move to DragonLib
    public static class TimeExtensions
    {
        public static string RelativeTime(this DateTimeOffset time, long target)
        {
            return time.RelativeTime(DateTimeOffset.FromUnixTimeMilliseconds(target));
        }
        
        public static string RelativeTime(this DateTimeOffset time, DateTimeOffset target)
        {
            var relative = time > target ? time - target : target - time;
            var text = target.ToString("f");
            switch (relative.TotalMinutes)
            {
                case < 1:
                    return "just now";
                case < 60:
                    text = $"{Math.Truncate(relative.TotalMinutes)} minutes";
                    break;
                default:
                {
                    if (relative.TotalHours < 23) text = $"{Math.Round(relative.TotalHours)} hours";
                    else if (relative.TotalDays < 28) text = $"{Math.Round(relative.TotalDays)} days";
                    else return $"on {text}";
                    break;
                }
            }

            return target > time ? $"in about {text}" : $"about {text} ago";
        }
    }
}
