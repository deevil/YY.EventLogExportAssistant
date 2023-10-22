using System;
using System.Collections.Generic;
using System.IO;
using YY.EventLogReaderAssistant.Models;

namespace YY.EventLogReaderAssistant
{
    internal class LogParserReferencesLGF
    {
        private static readonly Dictionary<Type, string> _mapTypeAndReferenceTypeId = new Dictionary<Type, string>();
        static LogParserReferencesLGF()
        {
            _mapTypeAndReferenceTypeId.Add(typeof(Users), "1");
            _mapTypeAndReferenceTypeId.Add(typeof(Computers), "2");
            _mapTypeAndReferenceTypeId.Add(typeof(Applications), "3");
            _mapTypeAndReferenceTypeId.Add(typeof(Events), "4");
            _mapTypeAndReferenceTypeId.Add(typeof(Metadata), "5");
            _mapTypeAndReferenceTypeId.Add(typeof(WorkServers), "6");
            _mapTypeAndReferenceTypeId.Add(typeof(PrimaryPorts), "7");
            _mapTypeAndReferenceTypeId.Add(typeof(SecondaryPorts), "8");
        }

        private readonly string[] _objectReferencesTexts;

        public LogParserReferencesLGF(EventLogLGFReader readerLGF)
        {
            string textReferencesData;
            using (FileStream fs = new FileStream(readerLGF.LogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader sr = new StreamReader(fs))
                textReferencesData = sr.ReadToEnd();

            int beginBlockIndex = textReferencesData.IndexOf("{", StringComparison.Ordinal);
            if (beginBlockIndex < 0)
                return;

            textReferencesData = textReferencesData.Substring(beginBlockIndex);
            _objectReferencesTexts = LogParserLGF.ParseEventLogString("{" + textReferencesData + "}");
        }

        public void ReadReferencesByType<T>(Dictionary<long, T> referenceCollection)
            where T : IReferenceObject, new()
        {
            referenceCollection.Clear();

            if (_objectReferencesTexts == null)
                return;

            if (_mapTypeAndReferenceTypeId.ContainsKey(typeof(T)) == false)
                return;
            string filterReferenceTypeId = _mapTypeAndReferenceTypeId[typeof(T)];
            
            foreach (string textObject in _objectReferencesTexts)
            {
                string[] parsedEventData = LogParserLGF.ParseEventLogString(textObject);
                if (parsedEventData != null)
                {
                    string referenceTypeId = parsedEventData[0];
                    if(filterReferenceTypeId != referenceTypeId)
                        continue;

                    IReferenceObject referenceObject = new T();
                    referenceObject.FillByStringParsedData(parsedEventData);
                    if (referenceCollection.ContainsKey(referenceObject.GetKeyValue()))
                        referenceCollection.Remove(referenceObject.GetKeyValue());
                    referenceCollection.Add(referenceObject.GetKeyValue(), (T)referenceObject);
                }
            }
        }
    }
}
