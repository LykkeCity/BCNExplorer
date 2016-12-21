using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Common;
using Core.Asset;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace AzureRepositories.Asset
{
    public class AssetScoreEntity : TableEntity, IAssetScore
    {
        IEnumerable<string> IAssetScore.AssetIds => JsonConvert.DeserializeObject<List<string>>(AssetIds);

        public string AssetIds { get; set; }
        public double Score { get; set; }

        public static string CreatePartitionKey()
        {
            return "ASE";
        }

        public static string CreateRowKey(IEnumerable<string> assetIds)
        {
            return string.Join("_", assetIds);
        }

        public static AssetScoreEntity Create(IAssetScore assetScore)
        {
            return new AssetScoreEntity
            {
                AssetIds = assetScore.AssetIds.ToJson(),
                Score = assetScore.Score,
                PartitionKey = CreatePartitionKey(),
                RowKey = CreateRowKey(assetScore.AssetIds)
            };
        }
    }

    public class AssetScoreRepository:IAssetScoreRepository
    {
        private readonly INoSQLTableStorage<AssetScoreEntity> _tableStorage;

        public AssetScoreRepository(INoSQLTableStorage<AssetScoreEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public  Task InsertOrReplaceAsync(IAssetScore assetScore)
        {
            return _tableStorage.InsertOrReplaceAsync(AssetScoreEntity.Create(assetScore));
        }

        public async Task<IEnumerable<IAssetScore>> GetAllAsync()
        {
            return await _tableStorage.GetDataAsync();
        }
    }
}
