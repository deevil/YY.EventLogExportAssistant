using System;

namespace YY.EventLogReaderAssistant.Tests.Helpers
{
    public static class StringExtensions
    {
        public static DateTime ToDateTime(this string sourceValue)
        {
            try
            {
                return DateTime.Parse(sourceValue);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }
    }
}
