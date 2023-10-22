using System;
using System.Collections.Generic;

namespace YY.EventLogReaderAssistant.Helpers
{
    internal static class EventLogRowPartLGFExtensions
    {
        private static readonly Dictionary<EventLogRowPartLGF, int> _partNumberCache = new Dictionary<EventLogRowPartLGF, int>();

        internal static int AsInt(this EventLogRowPartLGF value)
        {
            if (!_partNumberCache.TryGetValue(value, out var valueAsInt))
            {
                valueAsInt = (int)value;
                _partNumberCache.Add(value, valueAsInt);
            }

            return valueAsInt;
        }
        internal static string Parse(this EventLogRowPartLGF value, string[] sourceData)
        {
            return Parse<string>(value, sourceData);
        }
        internal static T Parse<T>(this EventLogRowPartLGF value, string[] sourceData) where T : IConvertible
        {
            int valueAsInt = value.AsInt();

            string parsedValueAsString = sourceData[valueAsInt];
            return (T)Convert.ChangeType(parsedValueAsString, typeof(T));
        }
    }
}
