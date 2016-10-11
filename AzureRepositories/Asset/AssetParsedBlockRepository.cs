using System;
using System.Threading.Tasks;
using AzureStorage;
using Core.Asset;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Asset
{
    public class AssetParsedBlockEntity : TableEntity, IAssetParsedBlock
    {
        public string BlockHash { get; set; }

        public static string CreateRowKey(string hash)
        {
            return hash;
        }

        public static string CreatePartitionKey()
        {
            return "APB";
        }

        public static AssetParsedBlockEntity Create(IAssetParsedBlock block)
        {
            return new AssetParsedBlockEntity
            {
                BlockHash = block.BlockHash,
                RowKey = CreateRowKey(block.BlockHash),
                PartitionKey = CreatePartitionKey()
            };
        }
    }

    public class AssetParsedBlockRepository:IAssetParsedBlockRepository
    {
        private readonly INoSQLTableStorage<AssetParsedBlockEntity> _tableStorage;

        public AssetParsedBlockRepository(INoSQLTableStorage<AssetParsedBlockEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<bool> IsBlockExistsAsync(IAssetParsedBlock block)
        {
            return _tableStorage.RecordExists(AssetParsedBlockEntity.Create(block));
        }

        public Task AddBlockAsync(IAssetParsedBlock block)
        {
            return _tableStorage.InsertOrReplaceAsync(AssetParsedBlockEntity.Create(block));
        }
    }
}
