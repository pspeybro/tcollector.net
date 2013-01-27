using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Extensions
{
    public static class DateTimeExtensions
    {
        public static double ToUTCSeconds(this System.DateTime dateTime)
        {
            TimeSpan span = (dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
            return span.TotalSeconds;
        }

    }

    public static class DateTimeOffsetExtensions
    {
        public static double ToUTCSeconds(this System.DateTimeOffset dateTimeOffset)
        {
            TimeSpan span = (dateTimeOffset.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
            return span.TotalSeconds;
        }

    }
}
