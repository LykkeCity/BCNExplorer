using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Core.Asset;
using Providers.Contracts.Asset;
using Providers.TransportTypes.Asset;

namespace Providers.Providers.Asset
{
    public class AssetProvider
    {
        private readonly CachedDataDictionary<string, AssetContract> _cacheDictionary;

        public AssetProvider(CachedDataDictionary<string, AssetContract> cacheDictionary)
        {
            _cacheDictionary = cacheDictionary;
        }

        public async Task<IAsset> GetAssetAsync(string assetId)
        {
            var assetContract = await _cacheDictionary.GetItemAsync(assetId);
            if (assetContract != null)
            {
                return AssetDefinition.Create(assetContract);
            }

            return null;
        }

        public async Task<IDictionary<string, AssetDefinition>> GetAssetDictionaryAsync()
        {
            return (await _cacheDictionary.GetDictionaryAsync())
                .ToDictionary(p => p.Key, p => AssetDefinition.Create(p.Value));
        } 
    }
}
