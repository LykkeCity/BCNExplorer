using System.Threading.Tasks;
using AzureStorage;
using Core.Asset;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.AssetCoinHolders
{
    public class AssetChangesParsedBlockEntity : TableEntity, IAssetChangesParsedBlock
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

        public static AssetChangesParsedBlockEntity Create(IAssetChangesParsedBlock block)
        {
            return new AssetChangesParsedBlockEntity
            {
                BlockHash = block.BlockHash,
                RowKey = CreateRowKey(block.BlockHash),
                PartitionKey = CreatePartitionKey()
            };
        }
    }

    public class AssetChangesParsedBlockRepository:IAssetChangesParsedBlockRepository
    {
        private readonly INoSQLTableStorage<AssetChangesParsedBlockEntity> _tableStorage;

        public AssetChangesParsedBlockRepository(INoSQLTableStorage<AssetChangesParsedBlockEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<bool> IsBlockExistsAsync(IAssetChangesParsedBlock block)
        {
            return _tableStorage.RecordExists(AssetChangesParsedBlockEntity.Create(block));
        }

        public Task AddBlockAsync(IAssetChangesParsedBlock block)
        {
            return _tableStorage.InsertOrReplaceAsync(AssetChangesParsedBlockEntity.Create(block));
        }
    }
}
