using System;
using System.Data.SqlTypes;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("YY.EventLogReaderAssistant.Tests")]
namespace YY.EventLogReaderAssistant.Helpers
{
    internal static class DateTimeExtensions
    {
        #region Public Methods

        public static DateTime? ToNullIfTooEarlyForDb(this DateTime date)
        {
            return (date >= (DateTime)SqlDateTime.MinValue) ? date : (DateTime?)null;
        }

        public static DateTime ToMinDateTimeIfNull(this DateTime? date)
        {
            return (date == null) ? DateTime.MinValue : (DateTime)date;
        }

        public static long ToLongDateTimeFormat(this DateTime date)
        {
            return (long)(date - DateTime.MinValue).TotalMilliseconds * 10;
        }

        public static long ToMilliseconds(this DateTime date)
        {
            return (long)date.ToUniversalTime().Subtract(
                new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                ).TotalMilliseconds;
        }

        #endregion
    }
}
