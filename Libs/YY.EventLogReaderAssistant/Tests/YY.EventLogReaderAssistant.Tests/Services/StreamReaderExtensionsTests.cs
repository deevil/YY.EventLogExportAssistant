using System.IO;
using Xunit;
using YY.EventLogReaderAssistant.Helpers;

namespace YY.EventLogReaderAssistant.Tests.Services
{
    public class StreamReaderExtensionsTests
    {
        #region Private Member Variables

        private readonly string _sampleDatabaseFile;

        #endregion

        #region Constructor

        public StreamReaderExtensionsTests()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            var sampleDataDirectory = Path.Combine(currentDirectory, "SampleData");
            _sampleDatabaseFile = Path.Combine(sampleDataDirectory, "LGFFormatEventLog", "1Cv8.lgf");
        }

        #endregion

        #region Public Methods

        [Fact]
        public void GetAndSetPosition_Test()
        {
            string checkString;
            string resultString;
            long position;

            using (StreamReader reader = new StreamReader(_sampleDatabaseFile))
            {
                reader.ReadLine();
                position = reader.GetPosition();
                checkString = reader.ReadLine();
            }

            using (StreamReader reader = new StreamReader(_sampleDatabaseFile))
            {
                reader.SetPosition(position);
                resultString = reader.ReadLine();
            }

            Assert.Equal(checkString, resultString);
        }

        [Fact]
        public void SkipLine_Test()
        {
            string checkString;
            string resultString;

            using (StreamReader reader = new StreamReader(_sampleDatabaseFile))
            {
                reader.SkipLine(10);
                checkString = reader.ReadLine();
            }

            using (StreamReader reader = new StreamReader(_sampleDatabaseFile))
            {
                reader.SkipLine(10);
                resultString = reader.ReadLine();
            }

            Assert.Equal(checkString, resultString);
        }

        #endregion
    }
}
