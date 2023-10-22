using System;
using System.IO;
using YY.EventLogReaderAssistant.Models;
using System.Runtime.CompilerServices;
using YY.EventLogReaderAssistant.EventArguments;

[assembly: InternalsVisibleTo("YY.EventLogReaderAssistant.Tests")]
namespace YY.EventLogReaderAssistant
{
    public abstract class EventLogReader : IEventLogReader, IDisposable
    {
        #region Public Static Methods

        public static EventLogReader CreateReader(string pathLogFile)
        {
            string logFileWithReferences = GetEventLogFileWithReferences(pathLogFile);
            if (File.Exists(logFileWithReferences))
            {
                FileInfo logFileInfo = new FileInfo(logFileWithReferences);
                string logFileExtension = logFileInfo.Extension.ToUpper();
                if (logFileExtension.EndsWith("LGF"))
                    return new EventLogLGFReader(logFileInfo.FullName);
                if (logFileExtension.EndsWith("LGD"))
                    return new EventLogLGDReader(logFileInfo.FullName);
            }
            throw new ArgumentException("Invalid log file path");
        }

        #endregion

        #region Private Static Methods

        private static string GetEventLogFileWithReferences(string pathLogFile)
        {
            FileAttributes attr = File.GetAttributes(pathLogFile);
            string logFileWithReferences;
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                logFileWithReferences = $"{pathLogFile}{Path.DirectorySeparatorChar}{@"1Cv8.lgf"}";
            else
            {
                var logFileInfo = new FileInfo(pathLogFile);
                logFileWithReferences = logFileInfo.FullName;
            }
            if (!File.Exists(logFileWithReferences))
                logFileWithReferences = $"{pathLogFile}{Path.DirectorySeparatorChar}{@"1Cv8.lgd"}";

            return logFileWithReferences;
        }

        #endregion

        #region Private Member Variables

        protected int _readDelayMs = 60000;
        protected readonly string _logFilePath;
        protected readonly string _logFileDirectoryPath;
        protected long _currentFileEventNumber;
        protected TimeZoneInfo _logTimeZoneInfo;
        protected DateTime _referencesReadDate;
        protected ReferencesData _referencesData;
        protected RowData _currentRow;

        #endregion

        #region Constructor

        internal EventLogReader()
        { }
        internal EventLogReader(string logFilePath)
        {
            _logFilePath = logFilePath;
            _logFileDirectoryPath = new FileInfo(_logFilePath).Directory?.FullName;
            _logTimeZoneInfo = TimeZoneInfo.Local;
            _referencesReadDate = DateTime.MinValue;
            ReadEventLogReferences();
        }

        #endregion

        #region Public Properties

        public DateTime ReferencesReadDate => _referencesReadDate;
        public string ReferencesHash => _referencesData?.GetReferencesHash();
        public ReferencesData References => _referencesData;
        public RowData CurrentRow => _currentRow;
        public long CurrentFileEventNumber => _currentFileEventNumber;
        public string LogFilePath => _logFilePath;
        public string LogFileDirectoryPath => _logFileDirectoryPath;
        public virtual string CurrentFile => null;

        #endregion

        #region Public Methods

        public virtual bool Read()
        {
            throw new NotImplementedException();
        }
        public virtual bool GoToEvent(long eventNumber)
        {
            throw new NotImplementedException();
        }
        public virtual EventLogPosition GetCurrentPosition()
        {
            throw new NotImplementedException();
        }
        public virtual void SetCurrentPosition(EventLogPosition newPosition)
        {
            throw new NotImplementedException();
        }
        public virtual long Count()
        {
            throw new NotImplementedException();
        }
        public virtual void Reset()
        {
            throw new NotImplementedException();
        }
        public virtual long FilesCount()
        {
            throw new NotImplementedException();
        }
        public virtual bool PreviousFile()
        {
            throw new NotImplementedException();
        }
        public virtual bool NextFile()
        {
            throw new NotImplementedException();
        }
        public virtual bool LastFile()
        {
            throw new NotImplementedException();
        }
        public virtual void SetDelayMs(int delay)
        {
            _readDelayMs = delay;
        }
        public void SetTimeZone(TimeZoneInfo timeZone)
        {
            _logTimeZoneInfo = timeZone;
        }
        public TimeZoneInfo GetTimeZone()
        {
            return _logTimeZoneInfo;
        }
        public virtual void Dispose()
        {
            _referencesData = null;
            _currentRow = null;
        }

        #endregion

        #region Private Methods

        protected virtual void ReadEventLogReferences()
        {
        }

        #endregion

        #region Events

        public delegate void BeforeReadFileHandler(EventLogReader sender, BeforeReadFileEventArgs args);
        public delegate void AfterReadFileHandler(EventLogReader sender, AfterReadFileEventArgs args);
        public delegate void BeforeReadEventHandler(EventLogReader sender, BeforeReadEventArgs args);
        public delegate void AfterReadEventHandler(EventLogReader sender, AfterReadEventArgs args);
        public delegate void OnErrorEventHandler(EventLogReader sender, OnErrorEventArgs args);

        public event BeforeReadFileHandler BeforeReadFile;
        public event AfterReadFileHandler AfterReadFile;
        public event BeforeReadEventHandler BeforeReadEvent;
        public event AfterReadEventHandler AfterReadEvent;
        public event OnErrorEventHandler OnErrorEvent;

        protected void RaiseBeforeReadFile(BeforeReadFileEventArgs args)
        {
            BeforeReadFile?.Invoke(this, args);
        }
        protected void RaiseAfterReadFile(AfterReadFileEventArgs args)
        {
            AfterReadFile?.Invoke(this, args);
        }
        protected void RaiseBeforeRead(BeforeReadEventArgs args)
        {
            BeforeReadEvent?.Invoke(this, args);
        }
        protected void RaiseAfterRead(AfterReadEventArgs args)
        {
            AfterReadEvent?.Invoke(this, args);
        }
        protected void RaiseOnError(OnErrorEventArgs args)
        {
            OnErrorEvent?.Invoke(this, args);
        }

        #endregion
    }
}
