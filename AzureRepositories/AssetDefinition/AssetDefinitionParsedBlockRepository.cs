using System.Threading.Tasks;
using AzureStorage;
using Core.Asset;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.AssetDefinition
{
    public class AssetDefinitionParsedBlockEntity : TableEntity, IAssetDefinitionParsedBlock
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

        public static AssetDefinitionParsedBlockEntity Create(IAssetDefinitionParsedBlock block)
        {
            return new AssetDefinitionParsedBlockEntity
            {
                BlockHash = block.BlockHash,
                RowKey = CreateRowKey(block.BlockHash),
                PartitionKey = CreatePartitionKey()
            };
        }
    }

    public class AssetDefinitionParsedBlockRepository:IAssetDefinitionParsedBlockRepository
    {
        private readonly INoSQLTableStorage<AssetDefinitionParsedBlockEntity> _tableStorage;

        public AssetDefinitionParsedBlockRepository(INoSQLTableStorage<AssetDefinitionParsedBlockEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<bool> IsBlockExistsAsync(IAssetDefinitionParsedBlock block)
        {
            return _tableStorage.RecordExists(AssetDefinitionParsedBlockEntity.Create(block));
        }

        public Task AddBlockAsync(IAssetDefinitionParsedBlock block)
        {
            return _tableStorage.InsertOrReplaceAsync(AssetDefinitionParsedBlockEntity.Create(block));
        }
    }
}
