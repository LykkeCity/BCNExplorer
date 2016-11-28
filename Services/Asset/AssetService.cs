using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Core.Asset;

namespace Services.Asset
{
    public class AssetService:IAssetService
    {
        private readonly CachedDataDictionary<string, IAsset> _cacheDictionary;

        public AssetService(CachedDataDictionary<string, IAsset> cacheDictionary)
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

        public async Task<IEnumerable<IAsset>> GetAssetsAsync()
        {
            return (await _cacheDictionary.GetDictionaryAsync()).Values.Distinct(new AssetDefinitionUrlEqualityComparer());
        }
    }
}
