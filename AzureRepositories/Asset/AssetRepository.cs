using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using AzureStorage;
using Common;
using Core.Asset;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.Asset
{
    public class AssetEntity:TableEntity, IAsset
    {
        public static string GeneratePartitionKey()
        {
            return "Asset";
        }
        
        public static string GenerateRowKey(IEnumerable<string> assetIds )
        {
            return string.Join("_", assetIds);
        }

        IEnumerable<string> IAsset.AssetIds => Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(AssetIds);
        public string AssetIds { get; set; }
        public string ContactUrl { get; set; }
        public string NameShort { get; set; }
        public string Name { get; set; }
        public string Issuer { get; set; }
        public string Description { get; set; }
        public string DescriptionMime { get; set; }
        public string Type { get; set; }
        public int Divisibility { get; set; }
        public bool LinkToWebsite { get; set; }
        public string IconUrl { get; set; }
        public string ImageUrl { get; set; }
        public string Version { get; set; }
        public string AssetDefinitionUrl { get; set; }

        public static AssetEntity Create(IAsset data)
        {
            return new AssetEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(data.AssetIds),
                AssetDefinitionUrl = data.AssetDefinitionUrl,
                ContactUrl = data.ContactUrl,
                NameShort = data.NameShort,
                Name = data.Name,
                Description = data.Description,
                DescriptionMime = data.DescriptionMime,
                Divisibility = data.Divisibility,
                IconUrl = data.IconUrl,
                ImageUrl = data.ImageUrl,
                Issuer = data.Issuer,
                LinkToWebsite = data.LinkToWebsite,
                Type = data.Type,
                Version = data.Version,
                AssetIds = data.AssetIds.ToJson()
            };
        }
    }

    public class AssetRepository:IAssetRepository
    {
        private readonly INoSQLTableStorage<AssetEntity> _assetTableStorage;

        public AssetRepository(INoSQLTableStorage<AssetEntity> assetTableStorage)
        {
            _assetTableStorage = assetTableStorage;
        }

        public async Task InsertOrReplaceAsync(IAsset[] assets)
        {
            await _assetTableStorage.InsertOrReplaceBatchAsync(assets.Select(AssetEntity.Create));
        }

        public async Task<IEnumerable<IAsset>> GetAllAsync()
        {
            return await _assetTableStorage.GetDataAsync(AssetEntity.GeneratePartitionKey());
        }
    }

}
