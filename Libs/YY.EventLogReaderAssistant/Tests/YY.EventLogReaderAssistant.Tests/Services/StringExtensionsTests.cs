using System;
using System.Text;
using Xunit;
using YY.EventLogReaderAssistant.Helpers;

namespace YY.EventLogReaderAssistant.Tests.Services
{
    public class StringExtensionsTests
    {
        #region Public Methods

        [Fact]
        public void From16To10_Test()
        {
            string sourceValue = "2438c75058600";
            long checkValue = 637220491200000;
            long resultValue = sourceValue.From16To10();

            Assert.Equal(checkValue, resultValue);
        }
        [Fact]
        public void RemoveQuotes_Test()
        {
            string sourceValue = "\"Hello, world!\"";
            string checkValue = "Hello, world!";
            string resultValue = sourceValue.RemoveQuotes();

            Assert.Equal(checkValue, resultValue);
        }

        [Fact]
        public void RemoveDoubleQuotes_1_Test()
        {
            string sourceValue = "\"Hello, world!\"";
            string checkValue = "Hello, world!";
            string resultValue = sourceValue.RemoveDoubleQuotes();

            Assert.Equal(checkValue, resultValue);
        }
        [Fact]
        public void RemoveDoubleQuotes_2_Test()
        {
            string sourceValue = "\"";
            string checkValue = "";
            string resultValue = sourceValue.RemoveDoubleQuotes();

            Assert.Equal(checkValue, resultValue);
        }
        [Fact]
        public void RemoveBraces_Test()
        {
            string sourceValue = "{Hello, world!}";
            string checkValue = "Hello, world!";
            string resultValue = sourceValue.RemoveBraces();

            Assert.Equal(checkValue, resultValue);
        }
        [Fact]
        public void ToInt32_Test()
        {
            string sourceValue = "12345";
            int checkValue = 12345;
            int resultValue = sourceValue.ToInt32();

            Assert.Equal(checkValue, resultValue);
        }
        [Fact]
        public void ToInt64_Test()
        {
            string sourceValue = "12345";
            long checkValue = 12345;
            long resultValue = sourceValue.ToInt64();

            Assert.Equal(checkValue, resultValue);
        }
        [Fact]
        public void FromWin1251ToUTF8_Test()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            string sourceText = "Р—Р°РєР°Р· Р·РІРѕРЅРєР° С‚РµС…РЅРёС‡РµСЃРєРѕР№ РїРѕРґРґРµСЂР¶РєРё";
            string checkValue = "Заказ звонка технической поддержки";

            string resultValue = sourceText.FromWin1251ToUtf8();

            Assert.Equal(checkValue, resultValue);
        }
        [Fact]
        public void ToGuid_WrongValue_Test()
        {
            string sourceValue = "I AM GUID!";
            Guid checkValue = Guid.Empty;
            Guid resultValue = sourceValue.ToGuid();

            Assert.Equal(checkValue, resultValue);
        }
        [Fact]
        public void ToGuid_CorrectValue_Test()
        {
            string sourceValue = "F9168C5E-CEB2-4faa-B6BF-329BF39FA1E4";
            Guid checkValue = new Guid(sourceValue);
            Guid resultValue = sourceValue.ToGuid();

            Assert.Equal(checkValue, resultValue);
        }

        #endregion
    }
}
