using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Core.TransactionCache;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.TransactionCache
{
    public class TransactionCacheItemEntity: TableEntity, ITransactionCacheItem
    {
        public string TransactionId { get; set; }
        public bool IsReceived { get; set; }
        public string BlockHash { get; set; }
        public int BlockHeight { get; set; }
        public string Address { get; set; }

        public static string GeneratePartitionKey(string address)
        {
            return address;
        }

        public static string GenerateRowKey(ITransactionCacheItem source)
        {
            return $"{int.MaxValue - source.BlockHeight}_{source.TransactionId}";
        }

        public static TransactionCacheItemEntity Create(ITransactionCacheItem source)
        {
            return new TransactionCacheItemEntity
            {
                BlockHeight = source.BlockHeight,
                Address = source.Address,
                BlockHash = source.BlockHash,
                IsReceived = source.IsReceived,
                TransactionId = source.TransactionId,
                PartitionKey = GeneratePartitionKey(source.Address),
                RowKey = GenerateRowKey(source)
            };
        }
    }

    public class TransactionCacheRepository:ITransactionCacheRepository
    {
        private readonly INoSQLTableStorage<TransactionCacheItemEntity> _tableStorage;

        public TransactionCacheRepository(INoSQLTableStorage<TransactionCacheItemEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public Task InsertOrReplaceAsync(IEnumerable<ITransactionCacheItem> transactions)
        {
            return _tableStorage.InsertOrReplaceBatchAsync(transactions.Select(TransactionCacheItemEntity.Create));
        }

        public async Task<IEnumerable<ITransactionCacheItem>> GetAsync(string address)
        {
            return await _tableStorage.GetDataAsync(TransactionCacheItemEntity.GeneratePartitionKey(address));
        }

        public async Task<int> GetLastCachedBlockHeight(string address)
        {
            var res = await _tableStorage.GetTopRecordAsync(TransactionCacheItemEntity.GeneratePartitionKey(address));

            return res?.BlockHeight ?? 0;
        }
    }
}
