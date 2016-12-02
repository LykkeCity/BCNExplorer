using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Common;
using Core.Asset;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace AzureRepositories.AssetCoinHolders
{
    public class AssetCoinholdersIndexEntity:TableEntity, IAssetCoinholdersIndex
    {
        IEnumerable<string> IAssetCoinholdersIndex.AssetIds => JsonConvert.DeserializeObject<List<string>>(AssetIds);
        IDictionary<string, double> IAssetCoinholdersIndex.BalanceAddressDictionary => JsonConvert.DeserializeObject<Dictionary<string, double>>(BalanceAddressDictionary);
        public string BalanceAddressDictionary { get; set; }
        public string AssetIds { get; set; }

        public static string GenerateRowKey(IEnumerable<string> assetIds)
        {
            return string.Join("_", assetIds);
        }

        public static string GeneratePartitionKey()
        {
            return "ACI";
        }

        public static AssetCoinholdersIndexEntity Create(IAssetCoinholdersIndex source)
        {
            return new AssetCoinholdersIndexEntity
            {
                AssetIds = source.AssetIds.ToJson(),
                RowKey = GenerateRowKey(source.AssetIds),
                PartitionKey = GeneratePartitionKey(),
                BalanceAddressDictionary = source.BalanceAddressDictionary.ToJson()
            };
        }
    }

    public class AssetCoinholdersIndexRepository: IAssetCoinholdersIndexRepository
    {
        private readonly INoSQLTableStorage<AssetCoinholdersIndexEntity> _tableStorage;

        public AssetCoinholdersIndexRepository(INoSQLTableStorage<AssetCoinholdersIndexEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task InserOrReplaceAsync(IAssetCoinholdersIndex index)
        {
            try
            {
                await _tableStorage.InsertOrReplaceAsync(AssetCoinholdersIndexEntity.Create(index));
            }
            catch (Exception)
            {
                
                throw;
            }

        }

        public async Task<IEnumerable<IAssetCoinholdersIndex>> GetAllAsync()
        {
            return await _tableStorage.GetDataAsync();
        }
    }
}
