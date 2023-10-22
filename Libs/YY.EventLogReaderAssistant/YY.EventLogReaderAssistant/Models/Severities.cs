using System;

namespace YY.EventLogReaderAssistant.Models
{
    [Serializable]
    public enum Severity
    {
        Unknown = 0,
        Information = 1,
        Warning = 2,
        Error = 3,
        Note = 4
    }
}
