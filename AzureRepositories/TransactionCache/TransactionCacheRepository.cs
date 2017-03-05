using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureStorage;
using Common;
using Core.TransactionCache;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.TransactionCache
{
    public class TransactionCacheItemEntity: TableEntity, ITransactionCacheItem
    {
        public string TransactionId { get; set; }
        public bool IsReceived { get; set; }
        public string BlockHash { get; set; }
        public int? BlockHeight { get; set; }
        public string Address => GetAddress(PartitionKey);

        public static string GeneratePartitionKey(string address)
        {
            return address;
        }

        private static string GetAddress(string partitionKey)
        {
            return partitionKey;
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

        public async Task InsertOrReplaceAsync(IEnumerable<ITransactionCacheItem> transactions)
        {
            var maxbatchSize = 99;

            var insertTasks = new List<Task>();

            var semaphore = new SemaphoreSlim(5);

            foreach (var txsBatch in transactions.Where(p => p.BlockHeight != null).OrderBy(p=>p.BlockHeight).Batch(maxbatchSize))
            {
                await semaphore.WaitAsync();
                var task = _tableStorage.InsertOrReplaceBatchAsync(txsBatch.Select(TransactionCacheItemEntity.Create))
                    .ContinueWith(p =>
                    {
                        semaphore.Release(1);
                    });

                insertTasks.Add(task);
            }

            await Task.WhenAll(insertTasks);
        }

        public async Task<IEnumerable<ITransactionCacheItem>> GetAsync(string address)
        {
            return await _tableStorage.GetDataAsync(TransactionCacheItemEntity.GeneratePartitionKey(address));
        }

        public async Task<ITransactionCacheItem> GetLastCachedTransaction(string address)
        {
            return await _tableStorage.GetTopRecordAsync(TransactionCacheItemEntity.GeneratePartitionKey(address));
        }
    }
}
