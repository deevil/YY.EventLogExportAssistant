using System.Data.SQLite;

namespace YY.EventLogReaderAssistant.Models
{
    public interface IReferenceObject
    {
        long GetKeyValue();
        void FillBySqliteReader(SQLiteDataReader reader);
        void FillByStringParsedData(string[] parsedEventData);
    }
}
