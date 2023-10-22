using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using YY.EventLogReaderAssistant.Helpers;
using YY.EventLogReaderAssistant.Models;

namespace YY.EventLogReaderAssistant.Tests.Services
{
    public class MD5HashGeneratorTests
    {
        #region Private Member Variables

        private readonly string _sampleDatabaseFileLGF;
        private readonly string _sampleDatabaseFileLgd;
        private readonly string _sampleDatabaseFileLgdReadRefferencesIfChanged;

        #endregion

        #region Constructor

        public MD5HashGeneratorTests()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            var sampleDataDirectory = Path.Combine(currentDirectory, "SampleData");
            _sampleDatabaseFileLGF = Path.Combine(sampleDataDirectory, "LGFFormatEventLog", "1Cv8.lgf");
            _sampleDatabaseFileLgd = Path.Combine(sampleDataDirectory, "SQLiteFormatEventLog", "1Cv8.lgd");
            _sampleDatabaseFileLgdReadRefferencesIfChanged = Path.Combine(
                sampleDataDirectory, "SQLiteFormatEventLog", "1Cv8_ReadRefferences_IfChanged_Test.lgd");
        }

        public string SampleDatabaseFileLGD_ReadRefferences_IfChanged => _sampleDatabaseFileLgdReadRefferencesIfChanged;
        public string SampleDatabaseFileLGD => _sampleDatabaseFileLgd;

        #endregion

        #region Public Methods

        [Fact]
        public void GetMD5Hash_Test()
        {
            ReferencesData data1;
            ReferencesData data2;

            using(EventLogReader reader = EventLogReader.CreateReader(_sampleDatabaseFileLGF))
            {
                data1 = ReferencesData.CreateFromReader(reader);
            }
            using (EventLogReader reader = EventLogReader.CreateReader(_sampleDatabaseFileLGF))
            {
                data2 = ReferencesData.CreateFromReader(reader);
            }

            string hashMD51 = MD5HashGenerator.GetMd5Hash(data1);
            string hashMD52 = MD5HashGenerator.GetMd5Hash(data2);

            Assert.Equal(hashMD51, hashMD52);

        }

        #endregion

        #region Service

        [Serializable]
        private class ReferencesData
        {
            #region  Public Static Methods
            
            public static ReferencesData CreateFromReader(EventLogReader reader)
            {
                List<Severity> severities = new List<Severity>
                {
                    Severity.Error,
                    Severity.Information,
                    Severity.Note,
                    Severity.Unknown,
                    Severity.Warning
                };

                List<TransactionStatus> transactionStatuses = new List<TransactionStatus>
                {
                    TransactionStatus.Committed,
                    TransactionStatus.NotApplicable,
                    TransactionStatus.RolledBack,
                    TransactionStatus.Unfinished,
                    TransactionStatus.Unknown
                };

                var referenceData = new ReferencesData()
                {
                    Applications = reader.References.Applications.Select(e => e.Value).ToList().AsReadOnly(),
                    Computers = reader.References.Computers.Select(e => e.Value).ToList().AsReadOnly(),
                    Events = reader.References.Events.Select(e => e.Value).ToList().AsReadOnly(),
                    Metadata = reader.References.Metadata.Select(e => e.Value).ToList().AsReadOnly(),
                    PrimaryPorts = reader.References.PrimaryPorts.Select(e => e.Value).ToList().AsReadOnly(),
                    SecondaryPorts = reader.References.SecondaryPorts.Select(e => e.Value).ToList().AsReadOnly(),
                    Users = reader.References.Users.Select(e => e.Value).ToList().AsReadOnly(),
                    WorkServers = reader.References.WorkServers.Select(e => e.Value).ToList().AsReadOnly(),
                    Severities = severities.ToList().AsReadOnly(),
                    TransactionStatuses = transactionStatuses.ToList().AsReadOnly()
                };

                return referenceData;
            }

            #endregion

            #region Public Members

            public IReadOnlyList<Applications> Applications { get; set; }
            public IReadOnlyList<Computers> Computers { get; set; }
            public IReadOnlyList<Events> Events { get; set; }
            public IReadOnlyList<Metadata> Metadata { get; set; }
            public IReadOnlyList<PrimaryPorts> PrimaryPorts { get; set; }
            public IReadOnlyList<SecondaryPorts> SecondaryPorts { get; set; }
            public IReadOnlyList<Severity> Severities { get; set; }
            public IReadOnlyList<TransactionStatus> TransactionStatuses { get; set; }
            public IReadOnlyList<Users> Users { get; set; }
            public IReadOnlyList<WorkServers> WorkServers { get; set; }

            #endregion

            #region Private Methods

            

            #endregion
        }

        #endregion
    }
}
