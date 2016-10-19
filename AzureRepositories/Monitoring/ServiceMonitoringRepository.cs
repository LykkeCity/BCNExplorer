using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Core.Monitoring;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Monitoring
{
    public class MonitoringRecordEntity : TableEntity, IMonitoringRecord
    {
        public static string GeneratePartitionKey()
        {
            return "Monitoring";
        }

        public static string GenerateRowKey(string serviceName)
        {
            return serviceName;
        }

        public static MonitoringRecordEntity Create(IMonitoringRecord record)
        {
            return new MonitoringRecordEntity
            {
                RowKey = GenerateRowKey(record.ServiceName),
                DateTime = record.DateTime,
                PartitionKey = GeneratePartitionKey()
            };
        }

        public string ServiceName => RowKey;
        public DateTime DateTime { get; set; }
    }

    public class ServiceMonitoringRepository : IServiceMonitoringRepository
    {
        readonly INoSQLTableStorage<MonitoringRecordEntity> _tableStorage;

        public ServiceMonitoringRepository(INoSQLTableStorage<MonitoringRecordEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public Task ScanAllAsync(Func<IEnumerable<IMonitoringRecord>, Task> chunk)
        {
            var partitionKey = MonitoringRecordEntity.GeneratePartitionKey();

            return _tableStorage.ScanDataAsync(partitionKey, chunk);
        }

        public Task UpdateOrCreate(IMonitoringRecord record)
        {
            var entity = MonitoringRecordEntity.Create(record);

            return _tableStorage.InsertOrReplaceAsync(entity);
        }
    }
}
