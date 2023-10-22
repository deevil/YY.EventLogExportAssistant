using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("YY.EventLogReaderAssistant.Tests")]
namespace YY.EventLogReaderAssistant.Helpers
{
    internal static class StringExtensions
    {
        #region Public Methods

        public static long From16To10(this string sourceValue)
        {
            return Convert.ToInt64(sourceValue.ToUpper(), 16);
        }
        public static string RemoveQuotes(this string sourceValue)
        {
            string functionReturnValue = sourceValue;

            if (functionReturnValue.StartsWith("\""))
            {
                functionReturnValue = functionReturnValue.Substring(1);
            }

            if (functionReturnValue.EndsWith("\""))
            {
                functionReturnValue = functionReturnValue.Substring(0, functionReturnValue.Length - 1);
            }

            return functionReturnValue;
        }
        public static string RemoveBraces(this string sourceString)
        {
            return sourceString.Replace("}", "").Replace("{", "");
        }
        public static int ToInt32(this string sourceString)
        {
            return Convert.ToInt32(sourceString);
        }
        public static long ToInt64(this string sourceString)
        {
            return Convert.ToInt64(sourceString);
        }
        public static string FromWin1251ToUtf8(this string sourceValue)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding utf8 = Encoding.GetEncoding("UTF-8");
            Encoding win1251 = Encoding.GetEncoding("windows-1251");

            return ConvertEncoding(sourceValue, win1251, utf8);
        }
        public static Guid ToGuid(this string sourceValue)
        {
            Guid.TryParse(sourceValue, out Guid guidFromString);
            return guidFromString;
        }
        public static string RemoveSpecialSymbols(this string sourceString)
        {
            char[] denied_nullChar = new[] { '\t', '\r' };
            char[] denied_whitespaceChar = new[] { '\n' };

            return RemoveSpecialSymbols(sourceString, denied_nullChar, denied_whitespaceChar);
        }
        public static string RemoveSpecialSymbols(this string sourceString, char[] deniedNullChars, char[] deniedWhitespaceChars)
        {
            string newString = string.Join("", sourceString
                .Select(c => deniedWhitespaceChars.Contains(c) ? ' ' : c)
                .Where(c => !deniedNullChars.Contains(c))
                .ToArray());

            return newString;
        }
        public static string RemoveCarriageReturnSymbol(this string sourceString)
        {
            char[] denied_nullChar = new[] { '\r' };
            char[] denied_whitespaceChar = new[] { ' ' };

            return RemoveSpecialSymbols(sourceString, denied_nullChar, denied_whitespaceChar);
        }
        public static string RemoveDoubleQuotes(this string sourceString)
        {
            if (sourceString.StartsWith("\"") && sourceString.EndsWith("\""))
            {
                if (sourceString.Length >= 3)
                    return sourceString.Substring(1, sourceString.Length - 2);
                return string.Empty;
            }
            else
                return sourceString;
        }

        #endregion

        #region Private Methods

        private static string ConvertEncoding(this string sourceString, Encoding source, Encoding result)
        {
            byte[] souceBytes = source.GetBytes(sourceString);
            byte[] resultBytes = Encoding.Convert(result, source, souceBytes);

            return source.GetString(resultBytes);
        }

        #endregion
    }
}
