using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("YY.EventLogReaderAssistant.Tests")]
namespace YY.EventLogReaderAssistant
{
    public sealed class EventLogPosition
    {
        #region Constructor

        public EventLogPosition(long eventNumber, string currentFileReferences, string currentFileData, long? streamPosition)
        {
            EventNumber = eventNumber;
            CurrentFileReferences = currentFileReferences;
            CurrentFileData = currentFileData;
            StreamPosition = streamPosition;
        }

        #endregion

        #region Public Properties

        public long EventNumber { get; }
        public string CurrentFileReferences { get; }
        public string CurrentFileData { get; }
        public long? StreamPosition { get; }

        #endregion
    }
}
