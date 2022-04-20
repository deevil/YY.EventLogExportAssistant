using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("YY.EventLogReaderAssistant.Tests")]
namespace YY.EventLogReaderAssistant.Helpers
{
    internal class StreamLineReader : IDisposable
    {
        #region Private Member Variables

        private const int BufferLength = 1024;
        private readonly byte[] _utf8Preamble = Encoding.UTF8.GetPreamble();
        private Stream _base;
        private readonly Encoding _encoding;
        private int _read, _index;
        private readonly byte[] _readBuffer = new byte[BufferLength];

        #endregion

        #region Constructor

        public StreamLineReader(Stream stream) : this(stream, Encoding.UTF8)
        {
        }

        public StreamLineReader(Stream stream, Encoding encoding)
        {
            _base = stream;
            _encoding = encoding;
        }

        #endregion

        #region Public Methods

        public long CurrentPosition { get; private set; }

        public long CurrentLine { get; private set; }

        public bool GoToLine(long goToLine)
        { 
            return GetCount(goToLine, true) == goToLine;
        }

        public long GetCount(long goToLine = 0) 
        { 
            return GetCount(goToLine, false);
        }

        public string ReadLine()
        {
            bool resultFound = false;

            List<byte> bufferConvertEncodingCollection = new List<byte>();
            while (!resultFound)
            {
                if (_read <= 0)
                {
                    _index = 0;
                    _read = _base.Read(_readBuffer, 0, BufferLength);
                    if (_read == 0)
                    {
                        if (bufferConvertEncodingCollection.Count > 0) 
                            break;

                        return null;
                    }
                }

                for (int max = _index + _read; _index < max;)
                {
                    char ch = (char)_readBuffer[_index];
                                        
                    _read--;
                    _index++;
                    CurrentPosition++;

                    if (ch == '\0' || ch == '\n')
                    {
                        resultFound = true;
                        break;
                    }
                    else if (ch == '\r')
                    {
                    } else
                    {
                        bufferConvertEncodingCollection.Add(_readBuffer[_index - 1]);
                    }
                }
            }

            byte[] bufferConvertEncoding = bufferConvertEncodingCollection.ToArray();
            string prepearedString = GetStringFromBuffer(bufferConvertEncoding, bufferConvertEncoding.Length);
            CurrentLine++;

            return prepearedString;
        }

        public void Dispose()
        {
            if (_base != null)
            {
                _base.Close();
                _base.Dispose();
                _base = null;
            }
        }

        #endregion

        #region Private Methods

        private long GetCount(long goToLine, bool stopWhenLine)
        {
            _base.Seek(0, SeekOrigin.Begin);
            CurrentPosition = 0;
            CurrentLine = 0;
            _index = 0;
            _read = 0;

            long savePosition = _base.Length;

            do
            {
                if (CurrentLine == goToLine)
                {
                    savePosition = CurrentPosition;
                    if (stopWhenLine) return CurrentLine;
                }
            }
            while (ReadLine() != null);

            long count = CurrentLine;

            CurrentLine = goToLine;
            _base.Seek(savePosition, SeekOrigin.Begin);

            return count;
        }

        private string GetStringFromBuffer(byte[] bufferString, int bufferConvertEncodingIndex)
        {
            int bufferSizeCopy = bufferConvertEncodingIndex;
            byte[] readyConvertData = new byte[bufferSizeCopy];
            Array.Copy(bufferString, 0, readyConvertData, 0, bufferSizeCopy);

            string prepearedString;
            if (Equals(_encoding, Encoding.UTF8) && ByteArrayStartsWith(readyConvertData, 0, _utf8Preamble))
            {
                prepearedString = _encoding.GetString(readyConvertData, _utf8Preamble.Length, readyConvertData.Length - _utf8Preamble.Length);
            }
            else
                prepearedString = _encoding.GetString(readyConvertData);

            return prepearedString;
        }

        private bool ByteArrayStartsWith(byte[] source, int offset, byte[] match)
        {
            if (match.Length > (source.Length - offset))
            {
                return false;
            }

            for (int i = 0; i < match.Length; i++)
            {
                if (source[offset + i] != match[i])
                {
                    return false;
                }
            }
            return true;
        }

        #endregion
    }
}
