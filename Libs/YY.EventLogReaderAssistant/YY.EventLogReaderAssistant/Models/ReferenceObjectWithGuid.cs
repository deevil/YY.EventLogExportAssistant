using System;
using System.Data.SQLite;
using YY.EventLogReaderAssistant.Helpers;

namespace YY.EventLogReaderAssistant.Models
{
    [Serializable]
    public abstract class ReferenceObjectWithGuid : ReferenceObject
    {
        #region Public Members

        public Guid Uuid { get; set; }

        #endregion

        #region Public Methods

        public override void FillBySqliteReader(SQLiteDataReader reader)
        {
            base.FillBySqliteReader(reader);
            Uuid = reader.GetString(2).ToGuid();
        }
        public override void FillByStringParsedData(string[] parsedEventData)
        {
            Code = parsedEventData[3].ToInt64();
            Uuid = parsedEventData[1].ToGuid();
            Name = parsedEventData[2];
        }

        #endregion
    }
}
