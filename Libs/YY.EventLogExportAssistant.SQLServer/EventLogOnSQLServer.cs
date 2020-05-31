﻿using EFCore.BulkExtensions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YY.EventLogExportAssistant.SQLServer.Models;
using YY.EventLogReaderAssistant;
using RowData = YY.EventLogReaderAssistant.Models.RowData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace YY.EventLogExportAssistant.SQLServer
{
    public class EventLogOnSQLServer : EventLogOnTarget
    {
        #region Private Member Variables

        private const int _defaultPortion = 1000;
        private readonly int _portion;
        private readonly DbContextOptions<EventLogContext> _databaseOptions;
        private InformationSystemsBase _system;
        private DateTime _maxPeriodRowData;

        private IReadOnlyList<Applications> cacheApplications;
        private IReadOnlyList<Computers> cacheComputers;
        private IReadOnlyList<Events> cacheEvents;
        private IReadOnlyList<Metadata> cacheMetadata;
        private IReadOnlyList<PrimaryPorts> cachePrimaryPorts;
        private IReadOnlyList<SecondaryPorts> cacheSecondaryPorts;
        private IReadOnlyList<Severities> cacheSeverities;
        private IReadOnlyList<TransactionStatuses> cacheTransactionStatuses;
        private IReadOnlyList<Users> cacheUsers;
        private IReadOnlyList<WorkServers> cacheWorkServers;

        #endregion

        #region Constructor

        public EventLogOnSQLServer() : this(null, _defaultPortion)
        {
            
        }
        public EventLogOnSQLServer(int portion) : this(null, portion)
        {
            _portion = portion;
        }
        public EventLogOnSQLServer(DbContextOptions<EventLogContext> databaseOptions, int portion)
        {
            _maxPeriodRowData = DateTime.MinValue;
            _portion = portion;
            if (databaseOptions == null)
            {
                IConfiguration Configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .Build();
                string connectionString = Configuration.GetConnectionString("EventLogDatabase");

                var optionsBuilder = new DbContextOptionsBuilder<EventLogContext>();
                optionsBuilder.UseSqlServer(connectionString);
                _databaseOptions = optionsBuilder.Options;
            }
            else
                _databaseOptions = databaseOptions;
        }

        #endregion

        #region Public Methods

        public override EventLogPosition GetLastPosition()
        {
            using (EventLogContext _context = new EventLogContext(_databaseOptions))
            {
                var lastLogFile = _context.LogFiles
                    .Where(e => e.InformationSystemId == _system.Id 
                        && e.Id == _context.LogFiles.Where(i => i.InformationSystemId == _system.Id).Max(m => m.Id))
                    .SingleOrDefault();

                if (lastLogFile == null)
                    return null;
                else
                    return new EventLogPosition(
                        lastLogFile.LastEventNumber,
                        lastLogFile.LastCurrentFileReferences,
                        lastLogFile.LastCurrentFileData,
                        lastLogFile.LastStreamPosition);
            }
        }
        public override void SaveLogPosition(FileInfo logFileInfo, EventLogPosition position)
        {
            using (EventLogContext _context = new EventLogContext(_databaseOptions))
            {
                LogFiles foundLogFile = _context.LogFiles
                .Where(l => l.InformationSystemId == _system.Id && l.FileName == logFileInfo.Name && l.CreateDate == logFileInfo.CreationTimeUtc)
                .FirstOrDefault();

                if (foundLogFile == null)
                {
                    _context.LogFiles.Add(new LogFiles()
                    {
                        InformationSystemId = _system.Id,
                        FileName = logFileInfo.Name,
                        CreateDate = logFileInfo.CreationTimeUtc,
                        ModificationDate = logFileInfo.LastWriteTimeUtc,
                        LastCurrentFileData = position.CurrentFileData,
                        LastCurrentFileReferences = position.CurrentFileReferences,
                        LastEventNumber = position.EventNumber,
                        LastStreamPosition = position.StreamPosition
                    });
                }
                else
                {
                    foundLogFile.ModificationDate = logFileInfo.LastWriteTimeUtc;
                    foundLogFile.LastCurrentFileData = position.CurrentFileData;
                    foundLogFile.LastCurrentFileReferences = position.CurrentFileReferences;
                    foundLogFile.LastEventNumber = position.EventNumber;
                    foundLogFile.LastStreamPosition = position.StreamPosition;
                    _context.Entry(foundLogFile).State = EntityState.Modified;
                }

                _context.SaveChanges();
            }
        }
        public override int GetPortionSize()
        {
            return _portion;
        }
        public override void Save(RowData rowData)
        {
            IList<RowData> rowsData = new List<RowData>
            {
                rowData
            };
            Save(rowsData);
        }
        public override void Save(IList<RowData> rowsData)
        {
            using (EventLogContext _context = new EventLogContext(_databaseOptions))
            {
                if (_maxPeriodRowData == DateTime.MinValue)
                {
                    Models.RowData firstRow = _context.RowsData.FirstOrDefault();
                    if (firstRow != null)
                    {
                        DateTimeOffset _maxPeriodRowDataTimeOffset = _context.RowsData
                            .Where(p => p.InformationSystemId == _system.Id)
                            .Max(m => m.Period);
                        _maxPeriodRowData = _maxPeriodRowDataTimeOffset.DateTime;
                    }
                }

                List<Models.RowData> newEntities = new List<Models.RowData>();
                foreach (var itemRow in rowsData)
                {
                    if (itemRow == null)
                        continue;
                    if (_maxPeriodRowData != DateTime.MinValue && itemRow.Period <= _maxPeriodRowData)
                    {
                        var checkExist = _context.RowsData
                            .Where(e => e.InformationSystemId == _system.Id && e.Period == itemRow.Period && e.Id == itemRow.RowID)
                            .FirstOrDefault();
                        if (checkExist != null)
                            continue;
                    }

                    long? rowApplicationId = null;
                    Applications rowApplication;
                    if (itemRow.Application != null)
                    {
                        rowApplication = cacheApplications
                            .Where(e => e.InformationSystemId == _system.Id && e.Name == itemRow.Application.Name)
                            .First();
                        rowApplicationId = rowApplication.Id;
                    }

                    long? rowComputerId = null;
                    Computers rowComputer;
                    if (itemRow.Computer != null)
                    {
                        rowComputer = cacheComputers
                            .Where(e => e.InformationSystemId == _system.Id && e.Name == itemRow.Computer.Name)
                            .First();
                        rowComputerId = rowComputer.Id;
                    }

                    long? rowEventId = null;
                    Events rowEvent;
                    if (itemRow.Event != null)
                    {
                        rowEvent = cacheEvents
                            .Where(e => e.InformationSystemId == _system.Id && e.Name == itemRow.Event.Name)
                            .First();
                        rowEventId = rowEvent.Id;
                    }

                    long? rowMetadataId = null;
                    Metadata rowMetadata;
                    if (itemRow.Metadata != null)
                    {
                        rowMetadata = cacheMetadata
                            .Where(e => e.InformationSystemId == _system.Id && e.Name == itemRow.Metadata.Name && e.Uuid == itemRow.Metadata.Uuid)
                            .First();
                        rowMetadataId = rowMetadata.Id;
                    }

                    long? rowPrimaryPortId = null;
                    PrimaryPorts rowPrimaryPort;
                    if (itemRow.PrimaryPort != null)
                    {
                        rowPrimaryPort = cachePrimaryPorts.Where(e => e.InformationSystemId == _system.Id && e.Name == itemRow.PrimaryPort.Name)
                            .First();
                        rowPrimaryPortId = rowPrimaryPort.Id;
                    }

                    long? rowSecondaryPortId = null;
                    SecondaryPorts rowSecondaryPort;
                    if (itemRow.SecondaryPort != null)
                    {
                        rowSecondaryPort = cacheSecondaryPorts
                            .Where(e => e.InformationSystemId == _system.Id && e.Name == itemRow.SecondaryPort.Name)
                            .First();
                        rowSecondaryPortId = rowSecondaryPort.Id;
                    }

                    Severities rowSeverity = cacheSeverities
                        .Where(e => e.InformationSystemId == _system.Id && e.Name == itemRow.Severity.ToString())
                        .First();
                    TransactionStatuses rowTransactionStatus = cacheTransactionStatuses
                        .Where(e => e.InformationSystemId == _system.Id && e.Name == itemRow.TransactionStatus.ToString())
                        .First();

                    long? rowUserId = null;
                    Users rowUser;
                    if (itemRow.User != null)
                    {
                        rowUser = cacheUsers
                            .Where(e => e.InformationSystemId == _system.Id && e.Name == itemRow.User.Name)
                            .First();
                        rowUserId = rowUser.Id;
                    }

                    long? rowWorkServerId = null;
                    WorkServers rowWorkServer;
                    if (itemRow.WorkServer != null)
                    {
                        rowWorkServer = cacheWorkServers
                            .Where(e => e.InformationSystemId == _system.Id && e.Name == itemRow.WorkServer.Name)
                            .First();
                        rowWorkServerId = rowWorkServer.Id;
                    }

                    Models.RowData rowData = new Models.RowData()
                    {
                        ApplicationId = rowApplicationId,
                        Comment = itemRow.Comment,
                        ComputerId = rowComputerId,
                        ConnectId = itemRow.ConnectId,
                        Data = itemRow.Data,
                        DataPresentation = itemRow.DataPresentation,
                        DataUUID = itemRow.DataUUID,
                        EventId = rowEventId,
                        Id = itemRow.RowID,
                        InformationSystemId = _system.Id,
                        MetadataId = rowMetadataId,
                        Period = itemRow.Period,
                        PrimaryPortId = rowPrimaryPortId,
                        SecondaryPortId = rowSecondaryPortId,
                        Session = itemRow.Session,
                        SeverityId = rowSeverity.Id,
                        TransactionDate = itemRow.TransactionDate,
                        TransactionId = itemRow.TransactionId,
                        TransactionStatusId = rowTransactionStatus.Id,
                        UserId = rowUserId,
                        WorkServerId = rowWorkServerId
                    };

                    newEntities.Add(rowData);
                }

                _context.BulkInsert(newEntities);
            }
        }
        public override void SetInformationSystem(InformationSystemsBase system)
        {
            using (EventLogContext _context = new EventLogContext(_databaseOptions))
            {
                InformationSystems existSystem = _context.InformationSystems.Where(e => e.Name == system.Name).FirstOrDefault();
                if (existSystem == null)
                {
                    _context.InformationSystems.Add(new InformationSystems()
                    {
                        Name = system.Name,
                        Description = system.Description
                    });
                    _context.SaveChanges();
                    existSystem = _context.InformationSystems.Where(e => e.Name == system.Name).FirstOrDefault();
                }
                else
                {
                    if (existSystem.Description != system.Description)
                    {
                        existSystem.Description = system.Description;
                        _context.Update(system);
                        _context.SaveChanges();
                    }
                }

                _system = existSystem;
            }
        }
        public override void UpdateReferences(ReferencesData data)
        {
            using (EventLogContext _context = new EventLogContext(_databaseOptions))
            {
                if (data.Applications != null)
                {
                    foreach (var itemApplication in data.Applications)
                    {
                        Applications foundApplication = _context.Applications
                            .Where(e => e.InformationSystemId == _system.Id && e.Name == itemApplication.Name)
                            .FirstOrDefault();
                        if (foundApplication == null)
                        {
                            _context.Applications.Add(new Applications()
                            {
                                InformationSystemId = _system.Id,
                                Name = itemApplication.Name
                            });
                        }
                    }
                }
                if (data.Computers != null)
                {
                    foreach (var itemComputer in data.Computers)
                    {
                        Computers foundComputer = _context.Computers
                            .Where(e => e.InformationSystemId == _system.Id && e.Name == itemComputer.Name)
                            .FirstOrDefault();
                        if (foundComputer == null)
                        {
                            _context.Computers.Add(new Computers()
                            {
                                InformationSystemId = _system.Id,
                                Name = itemComputer.Name
                            });
                        }
                    }
                }
                if (data.Events != null)
                {
                    foreach (var itemEvent in data.Events)
                    {
                        Events foundEvents = _context.Events
                            .Where(e => e.InformationSystemId == _system.Id && e.Name == itemEvent.Name)
                            .FirstOrDefault();
                        if (foundEvents == null)
                        {
                            _context.Events.Add(new Events()
                            {
                                InformationSystemId = _system.Id,
                                Name = itemEvent.Name
                            });
                        }
                    }
                }
                if (data.Metadata != null)
                {
                    foreach (var itemMetadata in data.Metadata)
                    {
                        Metadata foundMetadata = _context.Metadata
                            .Where(e => e.InformationSystemId == _system.Id
                                && e.Name == itemMetadata.Name
                                && e.Uuid == itemMetadata.Uuid)
                            .FirstOrDefault();
                        if (foundMetadata == null)
                        {
                            _context.Metadata.Add(new Metadata()
                            {
                                InformationSystemId = _system.Id,
                                Name = itemMetadata.Name,
                                Uuid = itemMetadata.Uuid
                            });
                        }
                    }
                }
                if (data.PrimaryPorts != null)
                {
                    foreach (var itemPrimaryPort in data.PrimaryPorts)
                    {
                        PrimaryPorts foundPrimaryPort = _context.PrimaryPorts
                            .Where(e => e.InformationSystemId == _system.Id && e.Name == itemPrimaryPort.Name)
                            .FirstOrDefault();
                        if (foundPrimaryPort == null)
                        {
                            _context.PrimaryPorts.Add(new PrimaryPorts()
                            {
                                InformationSystemId = _system.Id,
                                Name = itemPrimaryPort.Name
                            });
                        }
                    }
                }
                if (data.SecondaryPorts != null)
                {
                    foreach (var itemSecondaryPort in data.SecondaryPorts)
                    {
                        SecondaryPorts foundSecondaryPort = _context.SecondaryPorts
                            .Where(e => e.InformationSystemId == _system.Id && e.Name == itemSecondaryPort.Name)
                            .FirstOrDefault();
                        if (foundSecondaryPort == null)
                        {
                            _context.SecondaryPorts.Add(new SecondaryPorts()
                            {
                                InformationSystemId = _system.Id,
                                Name = itemSecondaryPort.Name
                            });
                        }
                    }
                }
                if (data.Severities != null)
                {
                    foreach (var itemSeverity in data.Severities)
                    {
                        Severities foundSeverity = _context.Severities
                            .Where(e => e.InformationSystemId == _system.Id && e.Name == itemSeverity.ToString())
                            .FirstOrDefault();
                        if (foundSeverity == null)
                        {
                            _context.Severities.Add(new Severities()
                            {
                                InformationSystemId = _system.Id,
                                Name = itemSeverity.ToString()
                            });
                        }
                    }
                }
                if (data.TransactionStatuses != null)
                {
                    foreach (var itemTransactionStatus in data.TransactionStatuses)
                    {
                        TransactionStatuses foundTransactionStatus = _context.TransactionStatuses
                            .Where(e => e.InformationSystemId == _system.Id && e.Name == itemTransactionStatus.ToString())
                            .FirstOrDefault();
                        if (foundTransactionStatus == null)
                        {
                            _context.TransactionStatuses.Add(new TransactionStatuses()
                            {
                                InformationSystemId = _system.Id,
                                Name = itemTransactionStatus.ToString()
                            });
                        }
                    }
                }
                if (data.Users != null)
                {
                    foreach (var itemUser in data.Users)
                    {
                        Users foundUsers = _context.Users
                            .Where(e => e.InformationSystemId == _system.Id
                                    && e.Name == itemUser.Name
                                    && e.Uuid == itemUser.Uuid)
                            .FirstOrDefault();
                        if (foundUsers == null)
                        {
                            _context.Users.Add(new Users()
                            {
                                InformationSystemId = _system.Id,
                                Name = itemUser.Name,
                                Uuid = itemUser.Uuid
                            });
                        }
                    }
                }
                if (data.WorkServers != null)
                {
                    foreach (var itemWorkServer in data.WorkServers)
                    {
                        WorkServers foundWorkServer = _context.WorkServers
                            .Where(e => e.InformationSystemId == _system.Id
                                    && e.Name == itemWorkServer.Name)
                            .FirstOrDefault();
                        if (foundWorkServer == null)
                        {
                            _context.WorkServers.Add(new WorkServers()
                            {
                                InformationSystemId = _system.Id,
                                Name = itemWorkServer.Name
                            });
                        }
                    }
                }

                _context.SaveChanges();

                cacheApplications = _context.Applications.ToList().AsReadOnly();
                cacheComputers = _context.Computers.ToList().AsReadOnly();
                cacheEvents = _context.Events.ToList().AsReadOnly();
                cacheMetadata = _context.Metadata.ToList().AsReadOnly();
                cachePrimaryPorts = _context.PrimaryPorts.ToList().AsReadOnly();
                cacheSecondaryPorts = _context.SecondaryPorts.ToList().AsReadOnly();
                cacheSeverities = _context.Severities.ToList().AsReadOnly();
                cacheTransactionStatuses = _context.TransactionStatuses.ToList().AsReadOnly();
                cacheUsers = _context.Users.ToList().AsReadOnly();
                cacheWorkServers = _context.WorkServers.ToList().AsReadOnly();
            }
        }

        #endregion
    }
}
