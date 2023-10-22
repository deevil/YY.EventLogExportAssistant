﻿using System;

namespace YY.EventLogReaderAssistant.EventArguments
{
    public sealed class BeforeReadEventArgs : EventArgs
    {
        public BeforeReadEventArgs(string sourceData, long eventNumber)
        {
            SourceData = sourceData;
            EventNumber = eventNumber;
        }

        public string SourceData { get; }
        public long EventNumber { get; }
    }    
}
