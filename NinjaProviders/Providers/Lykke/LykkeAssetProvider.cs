using System.Threading.Tasks;
using Common;
using Providers.Contracts.Lykke;
using Providers.TransportTypes.Lykke;

namespace Providers.Providers.Lykke
{
    public class LykkeAssetProvider
    {
        private readonly CachedDataDictionary<string, LykkeAssetContract> _cacheDictionary;

        public LykkeAssetProvider(CachedDataDictionary<string, LykkeAssetContract> cacheDictionary)
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
