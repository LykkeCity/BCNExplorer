using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureStorage;
using Core.TransactionCache;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.TransactionCache
{
    public class TransactionCachedStatusEntity : TableEntity, ITransactionCacheStatus
    {
        public static string GenerateRowKey(string address)
        {
            return address;
        }

        public static string GeneratePartitionKey()
        {
            return "TCS";
        }

        public int BlockHeight { get; set; }
        public string Address { get; set; }
        public bool FullLoaded { get; set; }

        public static TransactionCachedStatusEntity Create(string address, int blockHeight, bool fullLoaded)
        {
            return new TransactionCachedStatusEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(address),
                Address = address,
                BlockHeight = blockHeight,
                FullLoaded = fullLoaded
            };
        }
    }

    public class TransactionCachedStatusRepository:ITransactionCacheStatusRepository
    {
        private readonly INoSQLTableStorage<TransactionCachedStatusEntity> _tableStorage;

        public TransactionCachedStatusRepository(INoSQLTableStorage<TransactionCachedStatusEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<ITransactionCacheStatus> GetAsync(string address)
        {
            return await _tableStorage.GetDataAsync(
                TransactionCachedStatusEntity.GeneratePartitionKey(),
                TransactionCachedStatusEntity.GenerateRowKey(address));
        }

        public Task SetAsync(string address, int blockHeight, bool fullLoaded)
        {
            return _tableStorage.InsertOrReplaceAsync(TransactionCachedStatusEntity.Create(address, blockHeight, fullLoaded));
        }
    }
}
