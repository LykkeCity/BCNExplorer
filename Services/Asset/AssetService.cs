using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Core.Asset;

namespace Services.Asset
{
    public class AssetService:IAssetService
    {
        private readonly CachedDataDictionary<string, IAssetDefinition> _assetDefinitionCachedDictionary;
        private readonly CachedDataDictionary<string, IAssetCoinholdersIndex> _assetCoinholdersIndexesDictionary;

        public AssetService(CachedDataDictionary<string, IAssetDefinition> assetDefinitionCachedDictionary, 
            CachedDataDictionary<string, IAssetCoinholdersIndex> assetCoinholdersIndexesDictionary)
        {
            _assetDefinitionCachedDictionary = assetDefinitionCachedDictionary;
            _assetCoinholdersIndexesDictionary = assetCoinholdersIndexesDictionary;
        }

        public async Task<IAssetDefinition> GetAssetAsync(string assetId)
        {
            return await _assetDefinitionCachedDictionary.GetItemAsync(assetId);
        }

        public async Task<IAssetDefinition> GetAssetDefinitionByDefUrlAsync(string url)
        {
            return (await _assetDefinitionCachedDictionary.Values()).FirstOrDefault(p => p.AssetDefinitionUrl == url);
        }

        public async Task<IDictionary<string, IAssetDefinition>> GetAssetDefinitionDictionaryAsync()
        {
            return await _assetDefinitionCachedDictionary.GetDictionaryAsync();
        }

        public async Task<IDictionary<string, IAssetCoinholdersIndex>> GetAssetCoinholdersIndexAsync()
        {
            return await _assetCoinholdersIndexesDictionary.GetDictionaryAsync();
        }

        public async Task<IEnumerable<IAssetDefinition>> GetAssetDefinitionsAsync()
        {
            return (await _assetDefinitionCachedDictionary.GetDictionaryAsync()).Values.Distinct(new AssetDefinitionUrlEqualityComparer());
        }
    }
}
