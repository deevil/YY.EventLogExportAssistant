using System;
using System.Collections.Generic;
using System.Linq;
using YY.EventLogReaderAssistant.Helpers;
using YY.EventLogReaderAssistant.Models;

namespace YY.EventLogReaderAssistant
{
    [Serializable]
    public class ReferencesData
    {
        #region Constructor

        public ReferencesData()
        {
            _applications = new Dictionary<long, Applications>();
            _computers = new Dictionary<long, Computers>();
            _events = new Dictionary<long, Events>();
            _metadata = new Dictionary<long, Metadata>();
            _primaryPorts = new Dictionary<long, PrimaryPorts>();
            _secondaryPorts = new Dictionary<long, SecondaryPorts>();
            _users = new Dictionary<long, Users>();
            _workServers = new Dictionary<long, WorkServers>();
            _severity = new Dictionary<long, Severity>();
            _transactionStatus = new Dictionary<long, TransactionStatus>();
        }

        #endregion

        #region Private Members
        
        internal Dictionary<long, Applications> _applications;
        internal Dictionary<long, Computers> _computers;
        internal Dictionary<long, Events> _events;
        internal Dictionary<long, Metadata> _metadata;
        internal Dictionary<long, PrimaryPorts> _primaryPorts;
        internal Dictionary<long, SecondaryPorts> _secondaryPorts;
        internal Dictionary<long, Users> _users;
        internal Dictionary<long, WorkServers> _workServers;
        internal Dictionary<long, Severity> _severity;
        internal Dictionary<long, TransactionStatus> _transactionStatus;

        #endregion

        #region Public Members

        public IReadOnlyDictionary<long, Applications> Applications =>  _applications;
        public IReadOnlyDictionary<long, Computers> Computers => _computers;
        public IReadOnlyDictionary<long, Events> Events => _events;
        public IReadOnlyDictionary<long, Metadata> Metadata => _metadata;
        public IReadOnlyDictionary<long, PrimaryPorts> PrimaryPorts => _primaryPorts;
        public IReadOnlyDictionary<long, SecondaryPorts> SecondaryPorts => _secondaryPorts;
        public IReadOnlyDictionary<long, Users> Users => _users;
        public IReadOnlyDictionary<long, WorkServers> WorkServers => _workServers;
        public IReadOnlyDictionary<long, Severity> Severities => _severity;
        public IReadOnlyDictionary<long, TransactionStatus> TransactionStatuses => _transactionStatus;

        #endregion

        #region Public Methods

        public string GetReferencesHash()
        {
            return MD5HashGenerator.GetMd5Hash(this);
        }

        #endregion

        #region Private Methods

        private Dictionary<long, T> ListToDictionary<T>(IReadOnlyCollection<T> sourceList) where T : ReferenceObject, new()
        {
            Dictionary<long, T> resultDictionary = new Dictionary<long, T>();
            if (sourceList != null)
                resultDictionary = sourceList.ToDictionary(x => x.Code, x => x);
            return resultDictionary;
        }
        private Dictionary<long, T> EnumToDictionary<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T))
                .Cast<T>()
                .ToDictionary(t => (long)(object)t, t => t);
        }

        #endregion
    }
}
