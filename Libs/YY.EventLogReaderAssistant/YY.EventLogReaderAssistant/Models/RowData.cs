using System;
using System.Data.SQLite;
using System.Globalization;
using System.Text.RegularExpressions;
using YY.EventLogReaderAssistant.Helpers;

namespace YY.EventLogReaderAssistant.Models
{
    [Serializable]
    public class RowData
    {
        #region Private Static Members

        private static readonly Regex _regexDataUuid;

        #endregion

        #region Constructors

        static RowData()
        {
            _regexDataUuid = new Regex(@"[\d]+:[\dA-Za-zА-Яа-я]{32}");
        }

        #endregion

        #region Public Members

        public DateTime Period { get; set; }
        public DateTime PeriodUTC { get; set; }
        public long RowId { get; set; }
        public Severity Severity { get; set; }
        public long? ConnectId { get; set; }
        public long? Session { get; set; }
        public TransactionStatus TransactionStatus { get; set; }
        public DateTime? TransactionDate { get; set; }
        public long? TransactionId { get; set; }
        public string TransactionPresentation
        {
            get
            {
                if(TransactionDate != null && TransactionId != null)
                    return $"{TransactionDate?.ToString("dd.MM.yyyy HH:mm:ss")} ({TransactionId})";

                return string.Empty;
            }
        }
        public Users User { get; set; }
        public Computers Computer { get; set; }
        public Applications Application { get; set; }
        public Events Event { get; set; }
        public string Comment { get; set; }
        public Metadata Metadata { get; set; }
        public string Data { get; set; }
        public string DataUuid { get; set; }
        public string DataPresentation { get; set; }
        public WorkServers WorkServer { get; set; }
        public PrimaryPorts PrimaryPort { get; set; }
        public SecondaryPorts SecondaryPort { get; set; }

        #endregion

        #region Public Methods

        internal void FillBySqliteReader(EventLogLGDReader reader, SQLiteDataReader sqlReader)
        {
            DateTime rowPeriod = sqlReader.GetInt64OrDefault(1).ToDateTimeFormat();
            RowId = sqlReader.GetInt64OrDefault(0);
            Period = rowPeriod;
            PeriodUTC = TimeZoneInfo.ConvertTimeToUtc(rowPeriod, reader.GetTimeZone());
            ConnectId = sqlReader.GetInt64OrDefault(2);
            Session = sqlReader.GetInt64OrDefault(3);
            TransactionStatus = reader.GetTransactionStatus(sqlReader.GetInt64OrDefault(4));
            TransactionDate = sqlReader.GetInt64OrDefault(5).ToNullableDateTimeElFormat();
            TransactionId = sqlReader.GetInt64OrDefault(6);
            User = reader.GetUserByCode(sqlReader.GetInt64OrDefault(7));
            Computer = reader.GetComputerByCode(sqlReader.GetInt64OrDefault(8));
            Application = reader.GetApplicationByCode(sqlReader.GetInt64OrDefault(9));
            Event = reader.GetEventByCode(sqlReader.GetInt64OrDefault(10));
            PrimaryPort = reader.GetPrimaryPortByCode(sqlReader.GetInt64OrDefault(11));
            SecondaryPort = reader.GetSecondaryPortByCode(sqlReader.GetInt64OrDefault(12));
            WorkServer = reader.GetWorkServerByCode(sqlReader.GetInt64OrDefault(13));
            Severity = reader.GetSeverityByCode(sqlReader.GetInt64OrDefault(14));
            Comment = sqlReader.GetStringOrDefault(15);
            Data = sqlReader.GetStringOrDefault(16).FromWin1251ToUtf8();
            DataUuid = GetDataUuid(Data);
            DataPresentation = sqlReader.GetStringOrDefault(17);
            Metadata = reader.GetMetadataByCode(sqlReader.GetInt64OrDefault(18));
        }

        internal void FillByStringParsedData(EventLogLGFReader reader, string[] parseResult)
        {
            string transactionSourceString = EventLogRowPartLGF.TransactionData.Parse(parseResult)
                .RemoveBraces()
                .Trim();
            string rowPeriodAsString = EventLogRowPartLGF.Period.Parse(parseResult);
            DateTime rowPeriod = DateTime.ParseExact(rowPeriodAsString, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);

            RowId = reader.CurrentFileEventNumber;
            Period = rowPeriod;
            PeriodUTC = TimeZoneInfo.ConvertTimeToUtc(rowPeriod, reader.GetTimeZone());
            TransactionStatus = reader.GetTransactionStatus(EventLogRowPartLGF.TransactionStatus.Parse(parseResult));
            TransactionDate = GetTransactionDate(transactionSourceString);
            TransactionId = GetTransactionId(transactionSourceString);
            User = reader.GetUserByCode(EventLogRowPartLGF.User.Parse(parseResult));
            Computer = reader.GetComputerByCode(EventLogRowPartLGF.Computer.Parse(parseResult));
            Application = reader.GetApplicationByCode(EventLogRowPartLGF.Application.Parse(parseResult));
            ConnectId = EventLogRowPartLGF.ConnectId.Parse<int>(parseResult);
            Event = reader.GetEventByCode(EventLogRowPartLGF.Event.Parse(parseResult));
            Severity = reader.GetSeverityByCode(EventLogRowPartLGF.Severity.Parse(parseResult));
            Comment = EventLogRowPartLGF.Comment.Parse(parseResult).RemoveQuotes();
            Metadata = reader.GetMetadataByCode(EventLogRowPartLGF.Metadata.Parse(parseResult));
            Data = GetData(EventLogRowPartLGF.Data.Parse(parseResult));
            DataPresentation = EventLogRowPartLGF.DataPresentation.Parse(parseResult).RemoveQuotes();
            WorkServer = reader.GetWorkServerByCode(EventLogRowPartLGF.WorkServer.Parse(parseResult));
            PrimaryPort = reader.GetPrimaryPortByCode(EventLogRowPartLGF.PrimaryPort.Parse(parseResult));
            SecondaryPort = reader.GetSecondaryPortByCode(EventLogRowPartLGF.SecondaryPort.Parse(parseResult));
            Session = EventLogRowPartLGF.Session.Parse<long>(parseResult);
            DataUuid = GetDataUuid(Data);
        }

        #endregion

        #region Private Methods

        private string GetDataUuid(string sourceData)
        {
            string dataUuid = string.Empty;

            MatchCollection matches = _regexDataUuid.Matches(sourceData);
            if (matches.Count > 0)
            {
                string[] dataPartsUuid = sourceData.Split(':');
                dataUuid = dataPartsUuid.Length == 2 ? dataPartsUuid[1].Replace("}", string.Empty) : string.Empty;
            }

            return dataUuid;
        }
        private DateTime? GetTransactionDate(string sourceString)
        {
            DateTime? transactionDate = null;

            long transDate = sourceString.Substring(0, sourceString.IndexOf(",", StringComparison.Ordinal)).From16To10();
            try
            {
                if (transDate != 0) transactionDate = new DateTime().AddSeconds((double)transDate / 10000);
            }
            catch
            {
                transactionDate = null;
            }

            return transactionDate;
        }
        private long? GetTransactionId(string sourceString)
        {
            long? transactionId = sourceString.Substring(sourceString.IndexOf(",", StringComparison.Ordinal) + 1).From16To10();

            return transactionId;
        }
        private string GetData(string sourceString)
        {
            string data = sourceString;

            if (data == "{\"U\"}")
                data = string.Empty;

            else if (data.StartsWith("{"))
            {
                string[] parsedObjects = LogParserLGF.ParseEventLogString(data);
                if (parsedObjects != null && parsedObjects.Length == 2)
                {
                    if (parsedObjects[0] == "\"S\"" || parsedObjects[0] == "\"R\"")
                        data = parsedObjects[1].RemoveQuotes();
                }
            }

            return data;
        }

        #endregion
    }
}
