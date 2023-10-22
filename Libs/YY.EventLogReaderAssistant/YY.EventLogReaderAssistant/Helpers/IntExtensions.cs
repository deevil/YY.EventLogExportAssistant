using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("YY.EventLogReaderAssistant.Tests")]
namespace YY.EventLogReaderAssistant.Helpers
{
    internal static class IntExtensions
    {
        #region #region Public Methods

        public static DateTime ToDateTimeFormat(this long s)
        {
            return DateTime.MinValue.AddSeconds((double)s / 10000);
        }

        public static DateTime? ToNullableDateTimeElFormat(this long s)
        {
            if (s == 0)
                return null;
            else
                return DateTime.MinValue.AddSeconds((double)s / 10000);
        }

        #endregion
    }
}
