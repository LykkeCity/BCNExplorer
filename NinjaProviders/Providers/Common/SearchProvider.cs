using System.Threading.Tasks;
using Providers.Providers.Asset;
using Providers.Providers.Ninja;
using Providers.TransportTypes.Ninja;

namespace Providers.Providers.Common
{
    public class SearchProvider
    {
        private readonly Ninja.SearchProvider _searchProvider;
        private readonly AssetProvider _assetProvider;

        public SearchProvider(Ninja.SearchProvider searchProvider, AssetProvider assetProvider)
        {
            _searchProvider = searchProvider;
            _assetProvider = assetProvider;
        }

        public async Task<NinjaType?> GetTypeAsync(string id)
        {
            var result = await _searchProvider.GetTypeAsync(id);

            if (result == null)
            {
                var asset = await _assetProvider.GetAssetAsync(id);
                result = asset != null ? (NinjaType?)NinjaType.Asset : null;
            }

            return result;
        }
    }
}
