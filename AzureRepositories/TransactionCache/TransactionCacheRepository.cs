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
        public bool IsReceived { get; set; }
        public string TransactionId { get; set; }

        public int? BlockHeight => GetBlockHeight(RowKey);
        public string Address => GetAddress(PartitionKey);

        public static string GeneratePartitionKey(string address)
        {
            return address;
        }

        private static string GetAddress(string partitionKey)
        {
            return partitionKey;
        }

        private static int GetBlockHeight(string rowKey)
        {
            var value = int.Parse(rowKey.Split(RowKeySeparator)[0]);
            return int.MaxValue - value;
        }

        private const char RowKeySeparator = '_';

        public static string GenerateRowKey(ITransactionCacheItem source)
        {
            return $"{int.MaxValue - source.BlockHeight}{RowKeySeparator}{source.TransactionId}";
        }

        public static TransactionCacheItemEntity Create(ITransactionCacheItem source)
        {
            return new TransactionCacheItemEntity
            {
                IsReceived = source.IsReceived,
                PartitionKey = GeneratePartitionKey(source.Address),
                RowKey = GenerateRowKey(source),
                TransactionId = source.TransactionId
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
