using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using YY.EventLogReaderAssistant.EventArguments;
using YY.EventLogReaderAssistant.Helpers;
using YY.EventLogReaderAssistant.Models;

[assembly: InternalsVisibleTo("YY.EventLogReaderAssistant.Tests")]
namespace YY.EventLogReaderAssistant
{
    internal sealed class EventLogLGFReader : EventLogReader
    {
        #region Private Member Variables

        private const long DefaultBeginLineForLgf = 3;
        private int _indexCurrentFile;
        private string[] _logFilesWithData;
        private long _eventCount = -1;
        private string _sourceData;
        private bool _firstReadForFile = true;
        private long? _currentStreamPosition;


        private StreamReader _stream;
        private readonly StringBuilder _eventSource;

        private LogParserLGF _logParser;
        private LogParserLGF LogParser => _logParser ??= new LogParserLGF(this);

        #endregion

        #region Public Properties

        public override string CurrentFile
        {
            get
            {
                if (!LogFileByIndexExist())
                    return null;
                else
                    return _logFilesWithData[_indexCurrentFile];
            }
        }

        #endregion

        #region Constructor

        internal EventLogLGFReader(string logFilePath) : base(logFilePath)
        {
            _indexCurrentFile = 0;
            UpdateEventLogFilesList();
            _eventSource = new StringBuilder();            
        }

        #endregion

        #region Public Methods

        public override bool Read()
        {
            bool output = false;

            try
            {
                if (!InitializeReadFileStream())
                    return false;

                RaiseBeforeReadFileEvent(out bool cancelBeforeReadFile);
                if (cancelBeforeReadFile)
                {
                    NextFile();
                    return Read();
                }
                bool newLine = true;
                _currentStreamPosition = _stream?.GetPosition() ?? 0;

                DateTime maxLogPeriod = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _logTimeZoneInfo).AddMilliseconds((-1) * _readDelayMs);
                while (true)
                {
                    if (_firstReadForFile)
                    {
                        _sourceData = ReadSourceDataFromStream();
                        _firstReadForFile = false;
                    }

                    if (_sourceData == null)
                    {
                        NextFile();
                        output = Read();
                        break;
                    }

                    AddNewLineToSource(_sourceData, newLine);

                    long currentStreamPositionBeforeGoAheadRead = _stream?.GetPosition() ?? 0;
                    if (LogParserLGF.ItsEndOfEvent(_stream, CurrentFile, out _sourceData))
                    {
                        _currentFileEventNumber += 1;
                        _currentStreamPosition = currentStreamPositionBeforeGoAheadRead;
                        string preparedSourceData = _eventSource.ToString().Trim();

                        if (_sourceData == null)
                            _firstReadForFile = true;

                        RaiseBeforeRead(new BeforeReadEventArgs(preparedSourceData, _currentFileEventNumber));

                        try
                        {
                            RowData eventData = ReadRowData(preparedSourceData);
                            if (eventData.Period >= maxLogPeriod)
                            {
                                _currentRow = null;
                                break;
                            }

                            _currentRow = eventData;
                            RaiseAfterRead(new AfterReadEventArgs(_currentRow, _currentFileEventNumber));
                            output = true;
                            break;
                        }
                        catch (Exception ex)
                        {
                            _currentRow = null;
                            
                            RaiseOnError(new OnErrorEventArgs(ex, preparedSourceData, false, GetCurrentPosition()));
                            output = true;
                            break;
                        }
                    }

                    newLine = false;
                }
            }
            catch (Exception ex)
            {
                RaiseOnError(new OnErrorEventArgs(ex, null, true));
                _currentRow = null;
                output = false;
            }

            return output;
        }
        public override bool GoToEvent(long eventNumber)
        {
            Reset();

            int fileIndex = -1;
            long currentLineNumber = -1;
            long currentEventNumber = 0;
            bool moved = false;

            foreach (string logFile in _logFilesWithData)
            {
                fileIndex += 1;
                currentLineNumber = -1;

                IEnumerable<string> allLines = File.ReadLines(logFile);
                foreach (string line in allLines)
                {
                    currentLineNumber += 1;
                    if(LogParserLGF.ItsBeginOfEvent(line))                    
                    {
                        currentEventNumber += 1;
                    }

                    if (currentEventNumber == eventNumber)
                    {
                        moved = true;
                        break;
                    }
                }

                if (currentEventNumber == eventNumber)
                {
                    moved = true;
                    break;
                }
            }           
            
            if (moved && fileIndex >= 0 && currentLineNumber >= 0)
            {
                InitializeStream(currentLineNumber, fileIndex);
                _eventCount = eventNumber - 1;
                _currentFileEventNumber = eventNumber;

                return true;
            }
            else
            {
                return false;
            }
        }

        public override EventLogPosition GetCurrentPosition()
        {
            return new EventLogPosition(
                _currentFileEventNumber,
                _logFilePath,
                CurrentFile,
                _currentStreamPosition ?? 0);
        }
        public override void SetCurrentPosition(EventLogPosition newPosition)
        {
            if(ApplyEventLogPosition(newPosition) == false)
                return;
            
            InitializeStream(DefaultBeginLineForLgf, _indexCurrentFile);
            long beginReadPosition =_stream.GetPosition();
            long newStreamPosition = Math.Max(beginReadPosition, newPosition.StreamPosition ?? 0);

            long sourceStreamPosition = newStreamPosition;
            string currentFilePath = _logFilesWithData[_indexCurrentFile];            
            
            LogParserLGF.FixEventPosition(currentFilePath, ref newStreamPosition, sourceStreamPosition);

            if (newPosition.StreamPosition != null)
                _stream?.SetPosition(newStreamPosition);
        }
        public override long Count()
        {
            if(_eventCount < 0)
                _eventCount = GetEventCount();

            return _eventCount;
        }
        public override void Reset()
        {
            if (_stream != null)
            {
                _stream.Dispose();
                _stream = null;
            }

            _indexCurrentFile = 0;
            UpdateEventLogFilesList();
            _currentFileEventNumber = 0;
            _currentRow = null;
            _sourceData = null;
            _firstReadForFile = true;
            _currentStreamPosition = null;
        }
        public override long FilesCount()
        {
            return _logFilesWithData.LongLength;
        }
        public override bool PreviousFile()
        {
            return ChangeFileStep(-1);
        }
        public override bool NextFile()
        {
            return ChangeFileStep(1);
        }
        public override bool LastFile()
        {
            while (NextFile()) { }

            return PreviousFile();
        }
        public override void Dispose()
        {
            base.Dispose();

            if (_stream != null)
            {
                _stream.Dispose();
                _stream = null;
            }
        }
        protected override void ReadEventLogReferences()
        {
            DateTime beginReadReferences = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _logTimeZoneInfo);
            _referencesData = new ReferencesData();

            var referencesInfo = LogParser.GetEventLogReferences();
            referencesInfo.ReadReferencesByType(_referencesData._users);
            referencesInfo.ReadReferencesByType(_referencesData._computers);
            referencesInfo.ReadReferencesByType(_referencesData._applications);
            referencesInfo.ReadReferencesByType(_referencesData._events);
            referencesInfo.ReadReferencesByType(_referencesData._metadata);
            referencesInfo.ReadReferencesByType(_referencesData._workServers);
            referencesInfo.ReadReferencesByType(_referencesData._primaryPorts);
            referencesInfo.ReadReferencesByType(_referencesData._secondaryPorts);

            _referencesReadDate = beginReadReferences;

            base.ReadEventLogReferences();
        }

        #endregion

        #region Private Methods

        private void UpdateEventLogFilesList()
        {
            _logFilesWithData = Directory
                .GetFiles(_logFileDirectoryPath, "*.lgp")
                .OrderBy(i => i)
                .ToArray();
        }
        private void AddNewLineToSource(string sourceData, bool newLine)
        {
            if (newLine)
                _eventSource.Append(sourceData);
            else
            {
                _eventSource.AppendLine();
                _eventSource.Append(sourceData);
            }
        }
        private string ReadSourceDataFromStream()
        {
            string sourceData = _stream.ReadLineWithoutNull();

            if (sourceData == "," && LogParserLGF.NextLineIsBeginEvent(_stream, CurrentFile, out _))
                sourceData = _stream.ReadLineWithoutNull();

            return sourceData;
        }
        private void RaiseBeforeReadFileEvent(out bool cancel)
        {
            BeforeReadFileEventArgs beforeReadFileArgs = new BeforeReadFileEventArgs(CurrentFile);
            if (_currentFileEventNumber == 0)
                RaiseBeforeReadFile(beforeReadFileArgs);

            cancel = beforeReadFileArgs.Cancel;
        }
        private bool InitializeReadFileStream()
        {
            if (_stream == null)
            {
                if (!LogFileByIndexExist())
                {
                    _currentRow = null;
                    return false;
                }

                InitializeStream(DefaultBeginLineForLgf, _indexCurrentFile);
                _currentFileEventNumber = 0;
            }
            _eventSource.Clear();

            return true;
        }
        private RowData ReadRowData(string sourceData)
        {
            RowData eventData = LogParser.Parse(sourceData);

            if (eventData != null && eventData.Period >= ReferencesReadDate)
            {
                ReadEventLogReferences();
                eventData = LogParser.Parse(sourceData);
            }

            return eventData;
        }
        private bool ApplyEventLogPosition(EventLogPosition position)
        {
            Reset();

            if (position == null)
                return false;

            if (position.CurrentFileReferences != _logFilePath)
                throw new Exception("Invalid data file with references");

            int indexOfFileData = Array.IndexOf(_logFilesWithData, position.CurrentFileData);
            if (indexOfFileData < 0)
                throw new Exception("Invalid data file");

            _indexCurrentFile = indexOfFileData;
            _currentFileEventNumber = position.EventNumber;

            return true;
        }
        private void InitializeStream(long linesToSkip, int fileIndex = 0)
        {
            FileStream fs = new FileStream(_logFilesWithData[fileIndex], FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 20480);
            _stream = new StreamReader(fs);
            _stream.SkipLine(linesToSkip);

            _currentStreamPosition = _stream?.GetPosition();
        }
        private long GetEventCount()
        {
            long eventCount = 0;

            foreach (var logFile in _logFilesWithData)
            {
                using (StreamReader logFileStream = new StreamReader(File.Open(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    do
                    {
                        string logFileCurrentString = logFileStream.ReadLineWithoutNull();
                        if (LogParserLGF.ItsBeginOfEvent(logFileCurrentString))
                            eventCount++;
                    } while (!logFileStream.EndOfStream);
                }
            }

            return eventCount;
        }
        private bool LogFileByIndexExist()
        {
            return _indexCurrentFile < _logFilesWithData.Length
                && _indexCurrentFile >= 0;
        }
        private bool ChangeFileStep(int fileIndexStepToChange)
        {
            RaiseAfterReadFileIfIsNecessary();

            if (_stream != null)
            {
                _stream.Dispose();
                _stream = null;
            }

            _indexCurrentFile += fileIndexStepToChange;
            _currentFileEventNumber = 0;
            _eventCount = -1;
            _firstReadForFile = true;

            return LogFileByIndexExist();
        }
        private void RaiseAfterReadFileIfIsNecessary()
        {
            if (_stream != null)
                RaiseAfterReadFile(new AfterReadFileEventArgs(CurrentFile));
        }

        #endregion
    }
}
