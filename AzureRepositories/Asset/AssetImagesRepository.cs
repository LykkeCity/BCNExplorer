using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Common;
using Core.Asset;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Asset
{
    public class AssetImageImageEntity:TableEntity, IAssetImage
    {
        public static string GeneratePartitionKey()
        {
            return "AssetImage";
        }

        public static string GenerateRowKey(IEnumerable<string> assetIds )
        {
            return string.Join("_", assetIds);
        }

        IEnumerable<string> IAssetImage.AssetIds => Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(AssetIds);
        public string AssetIds { get; set; }

        public string IconUrl { get; set; }
        public string ImageUrl { get; set; }


        public static AssetImageImageEntity Create(IAssetImage source)
        {
            return new AssetImageImageEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(source.AssetIds),
                ImageUrl = source.ImageUrl,
                IconUrl = source.IconUrl,
                AssetIds = source.AssetIds.ToJson()
            };
        }
    }

    public class AssetImageRepository:IAssetImageRepository
    {
        private readonly INoSQLTableStorage<AssetImageImageEntity> _tableStorage;

        public AssetImageRepository(INoSQLTableStorage<AssetImageImageEntity> assetTableStorage)
        {
            _tableStorage = assetTableStorage;
        }

        public async Task<IEnumerable<IAssetImage>> GetAllAsync()
        {
            return await _tableStorage.GetDataAsync();
        }

        public async Task InsertOrReplaceAsync(IAssetImage assetImage)
        {
            await _tableStorage.InsertOrReplaceAsync(AssetImageImageEntity.Create(assetImage));
        }
    }
}
