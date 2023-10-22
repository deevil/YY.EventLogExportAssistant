using System;
using Xunit;
using YY.EventLogReaderAssistant.Helpers;

namespace YY.EventLogReaderAssistant.Tests.Services
{
    public class DateTimeExtensionsTests
    {
        #region Public Methods

        [Fact]
        public void ToNullIfTooEarlyForDb_IsNull_Test()
        {
            DateTime sourceDate = new DateTime(1000, 1, 1);
            DateTime? resultDate = sourceDate.ToNullIfTooEarlyForDb();

            Assert.Null(resultDate);
        }

        [Fact]
        public void ToNullIfTooEarlyForDb_IsNotNull_Test()
        {
            DateTime sourceDate = DateTime.Now;
            DateTime? resultDate = sourceDate.ToNullIfTooEarlyForDb();

            Assert.Equal(sourceDate, resultDate);
        }

        [Fact]
        public void ToLongDateTimeFormat_Test()
        {
            DateTime sourceDate = new DateTime(2020, 1, 19);
            long resultDate = sourceDate.ToLongDateTimeFormat();
            long correctDate = 637149888000000;

            Assert.Equal(correctDate, resultDate);
        }

        [Fact]
        public void ToMilliseconds_Test()
        {
            DateTime sourceDate = new DateTime(2020, 1, 19);
            DateTimeOffset dto = new DateTimeOffset(sourceDate);

            long resultDate = sourceDate.ToMilliseconds();            
            long correctDate = dto.ToUnixTimeMilliseconds();

            Assert.Equal(correctDate, resultDate);
        }

        #endregion
    }
}
