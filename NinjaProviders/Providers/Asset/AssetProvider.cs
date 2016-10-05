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
        private readonly CachedDataDictionary<string, IAsset> _cacheDictionary;

        public AssetProvider(CachedDataDictionary<string, IAsset> cacheDictionary)
        {
            _cacheDictionary = cacheDictionary;
        }

        public async Task<IAsset> GetAssetAsync(string assetId)
        {
            return await _cacheDictionary.GetItemAsync(assetId);
        }

        public async Task<IDictionary<string, IAsset>> GetAssetDictionaryAsync()
        {
            return await _cacheDictionary.GetDictionaryAsync();
        } 
    }
}
