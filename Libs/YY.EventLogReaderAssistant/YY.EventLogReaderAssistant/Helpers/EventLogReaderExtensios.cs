using YY.EventLogReaderAssistant.Models;

namespace YY.EventLogReaderAssistant.Helpers
{
    public static class EventLogReaderExtensions
    {
        public static Severity GetSeverityByCode(this EventLogReader reader, string code)
        {
            Severity severity;

            switch (code.Trim())
            {
                case "I":
                    severity = Severity.Information;
                    break;
                case "W":
                    severity = Severity.Warning;
                    break;
                case "E":
                    severity = Severity.Error;
                    break;
                case "N":
                    severity = Severity.Note;
                    break;
                default:
                    severity = Severity.Unknown;
                    break;
            }

            return severity;
        }
        public static Severity GetSeverityByCode(this EventLogReader reader, long code)
        {
            try
            {
                return (Severity)code;
            }
            catch
            {
                return Severity.Unknown;
            }
        }
        public static TransactionStatus GetTransactionStatus(this EventLogReader reader, string code)
        {
            TransactionStatus transactionStatus;

            if (code == "R")
                transactionStatus = TransactionStatus.Unfinished;
            else if (code == "N")
                transactionStatus = TransactionStatus.NotApplicable;
            else if (code == "U")
                transactionStatus = TransactionStatus.Committed;
            else if (code == "C")
                transactionStatus = TransactionStatus.RolledBack;
            else
                transactionStatus = TransactionStatus.Unknown;

            return transactionStatus;
        }
        public static TransactionStatus GetTransactionStatus(this EventLogReader reader, long code)
        {
            try
            {
                return (TransactionStatus)code;
            }
            catch
            {
                return TransactionStatus.Unknown;
            }
        }
        public static Users GetUserByCode(this EventLogReader reader, string code)
        {
            return GetUserByCode(reader, code.ToInt64());
        }
        public static Users GetUserByCode(this EventLogReader reader, long code)
        {
            reader.References.Users.TryGetValue(code, out var output);
            return output;
        }
        public static Computers GetComputerByCode(this EventLogReader reader, string code)
        {
            return GetComputerByCode(reader, code.ToInt64());
        }
        public static Computers GetComputerByCode(this EventLogReader reader, long code)
        {
            reader.References.Computers.TryGetValue(code, out var output);
            return output;
        }
        public static Applications GetApplicationByCode(this EventLogReader reader, string code)
        {
            return GetApplicationByCode(reader, code.ToInt64());
        }
        public static Applications GetApplicationByCode(this EventLogReader reader, long code)
        {
            reader.References.Applications.TryGetValue(code, out var output);
            return output;
        }
        public static Events GetEventByCode(this EventLogReader reader, string code)
        {
            return GetEventByCode(reader, code.ToInt64());
        }
        public static Events GetEventByCode(this EventLogReader reader, long code)
        {
            reader.References.Events.TryGetValue(code, out var output);
            return output;
        }
        public static Metadata GetMetadataByCode(this EventLogReader reader, string code)
        {
            return GetMetadataByCode(reader, code.ToInt64());
        }
        public static Metadata GetMetadataByCode(this EventLogReader reader, long code)
        {
            reader.References.Metadata.TryGetValue(code, out var output);
            return output;
        }
        public static WorkServers GetWorkServerByCode(this EventLogReader reader, string code)
        {
            return GetWorkServerByCode(reader, code.ToInt64());
        }
        public static WorkServers GetWorkServerByCode(this EventLogReader reader, long code)
        {
            reader.References.WorkServers.TryGetValue(code, out var output);
            return output;
        }
        public static PrimaryPorts GetPrimaryPortByCode(this EventLogReader reader, string code)
        {
            return GetPrimaryPortByCode(reader, code.ToInt64());
        }
        public static PrimaryPorts GetPrimaryPortByCode(this EventLogReader reader, long code)
        {
            reader.References.PrimaryPorts.TryGetValue(code, out var output);
            return output;
        }
        public static SecondaryPorts GetSecondaryPortByCode(this EventLogReader reader, string code)
        {
            return GetSecondaryPortByCode(reader, code.ToInt64());
        }
        public static SecondaryPorts GetSecondaryPortByCode(this EventLogReader reader, long code)
        {
            reader.References.SecondaryPorts.TryGetValue(code, out var output);
            return output;
        }
    }
}
