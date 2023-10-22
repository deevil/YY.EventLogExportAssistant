using System;

namespace YY.EventLogReaderAssistant.EventArguments
{
    public sealed class OnErrorEventArgs : EventArgs
    {
        #region Constructors

        public OnErrorEventArgs(Exception exception, string sourceData, bool critical)
        {
            Exception = exception;
            SourceData = sourceData;
            Critical = critical;
            BeginEventPosition = null;
        }
        public OnErrorEventArgs(Exception exception, string sourceData, bool critical, EventLogPosition beginEventPosition)
            :this(exception, sourceData, critical)
        {
            BeginEventPosition = beginEventPosition;
        }

        #endregion

        #region Public Members

        public EventLogPosition BeginEventPosition { get; }
        public Exception Exception { get; }
        public string SourceData { get; }
        public bool Critical { get; }

        #endregion
    }    
}
