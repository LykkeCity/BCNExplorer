using System.Threading.Tasks;
using Providers.Providers.Asset;
using Providers.Providers.Ninja;
using Providers.TransportTypes.Ninja;

namespace Providers.Providers.Common
{
    public class SearchProvider
    {
        private readonly NinjaSearchProvider _ninjaSearchProvider;
        private readonly AssetProvider _assetProvider;

        public SearchProvider(NinjaSearchProvider ninjaSearchProvider, AssetProvider assetProvider)
        {
            _ninjaSearchProvider = ninjaSearchProvider;
            _assetProvider = assetProvider;
        }

        public async Task<NinjaType?> GetTypeAsync(string id)
        {
            var result = await _ninjaSearchProvider.GetTypeAsync(id);

            if (result == null)
            {
                var asset = await _assetProvider.GetAssetAsync(id);
                result = asset != null ? (NinjaType?)NinjaType.Asset : null;
            }

            return result;
        }
    }
}
