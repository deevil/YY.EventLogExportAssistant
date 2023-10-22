using System;

namespace YY.EventLogReaderAssistant.Models
{
    [Serializable]
    public enum TransactionStatus
    {
        Unknown = 0,
        Committed = 1,
        Unfinished = 2,
        NotApplicable = 3,
        RolledBack = 4
    }
}
