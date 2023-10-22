using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using YY.EventLogReaderAssistant.Helpers;

namespace YY.EventLogReaderAssistant.Tests.Services
{
    public class StreamLineReaderTests
    {
        #region Private Member Variables

        private readonly string _sampleDatabaseFile;
        private readonly string[] _sampleFilesLgp;

        #endregion

        #region Constructor

        public StreamLineReaderTests()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            var sampleDataDirectory = Path.Combine(currentDirectory, "SampleData");
            _sampleDatabaseFile = Path.Combine(sampleDataDirectory, "LGFFormatEventLog", "1Cv8.lgf");

            string dataDirectoryLgf = Path.Combine(sampleDataDirectory, "LGFFormatEventLog");
            _sampleFilesLgp = Directory.GetFiles(dataDirectoryLgf, "*.lgp");
        }

        #endregion

        #region Public Methods

        [Fact]
        public void GoToLine_Test()
        {
            string lineContent = string.Empty;
            using (StreamReader reader = new StreamReader(_sampleDatabaseFile))
            {
                using StreamLineReader lineReader = new StreamLineReader(reader.BaseStream, reader.CurrentEncoding);
                if (lineReader.GoToLine(1))
                {
                    lineContent = lineReader.ReadLine();
                }
            }
            Guid.TryParse(lineContent, out Guid eventLogGuid);

            Assert.NotEqual(Guid.Empty, eventLogGuid);
        }

        [Fact]
        public void GetCount_Test()
        {
            long lineCounterNative = 0;
            using (var reader = new StreamReader(_sampleDatabaseFile))
            {
                while (reader.ReadLine() != null)
                {
                    lineCounterNative++;
                }
            }

            long lineCounterLibrary;
            using (StreamReader reader = new StreamReader(_sampleDatabaseFile))
            {
                using StreamLineReader lineReader = new StreamLineReader(reader.BaseStream, reader.CurrentEncoding);
                lineCounterLibrary = lineReader.GetCount();
            }

            Assert.Equal(lineCounterNative, lineCounterLibrary);
        }

        [Fact]
        public void ReadLine_LGP_Test()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding utf8 = Encoding.UTF8;

            List<string> correctLines = new List<string>();
            using (var reader = new StreamReader(_sampleDatabaseFile, utf8))
            {
                string currentLine = reader.ReadLine();
                do
                {
                    correctLines.Add(currentLine);
                    currentLine = reader.ReadLine();
                } while (currentLine != null);
            }

            List<string> resultLines = new List<string>();
            using (StreamReader reader = new StreamReader(_sampleDatabaseFile, utf8))
            {
                using StreamLineReader lineReader = new StreamLineReader(reader.BaseStream, reader.CurrentEncoding);
                string currentLine = lineReader.ReadLine();
                do
                {
                    resultLines.Add(currentLine);
                    currentLine = lineReader.ReadLine();
                } while (currentLine != null);
            }

            Assert.Equal(correctLines, resultLines);
        }

        [Fact]
        public void ReadLine_LGFs_Test()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding utf8 = Encoding.UTF8;

            List<string> correctLines = new List<string>();
            List<string> resultLines = new List<string>();

            foreach (string fileLgp in _sampleFilesLgp)
            {               
                using (var reader = new StreamReader(fileLgp, utf8))
                {
                    string currentLine = reader.ReadLine();
                    while (currentLine != null)
                    {
                        correctLines.Add(currentLine);
                        currentLine = reader.ReadLine();
                    }
                }
                
                using (StreamReader reader = new StreamReader(fileLgp, utf8))
                {
                    using StreamLineReader lineReader = new StreamLineReader(reader.BaseStream, reader.CurrentEncoding);
                    string currentLine = lineReader.ReadLine();
                    while (currentLine != null)
                    {
                        resultLines.Add(currentLine);
                        currentLine = lineReader.ReadLine();
                    }
                }
            }

            Assert.Equal(correctLines, resultLines);
        }

        #endregion
    }
}
