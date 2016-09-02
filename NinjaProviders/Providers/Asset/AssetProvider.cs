using System.Threading.Tasks;
using Common;
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

        public async Task<LykkeAsset> GetAssetAsync(string assetId)
        {
            var assetContract = await _cacheDictionary.GetItemAsync(assetId);
            if (assetContract != null)
            {
                return LykkeAsset.Create(assetContract);
            }

            return null;
        } 
    }
}
