using System.Threading.Tasks;
using Core.Asset;
using Core.SearchService;

namespace Services.Search
{
    public class SearchService:ISearchService
    {
        private readonly Providers.Providers.Ninja.NinjaSearchProvider _ninjaSearchProvider;
        private readonly IAssetService _assetProvider;

        public SearchService(Providers.Providers.Ninja.NinjaSearchProvider ninjaSearchProvider, IAssetService assetProvider)
        {
            _ninjaSearchProvider = ninjaSearchProvider;
            _assetProvider = assetProvider;
        }

        public async Task<SearchResultType?> GetTypeAsync(string id)
        {
            var result = await _ninjaSearchProvider.GetTypeAsync(id);

            if (result == null)
            {
                var asset = await _assetProvider.GetAssetAsync(id);
                result = asset != null ? (SearchResultType?)SearchResultType.Asset : null;
            }

            return result;
        }
    }
}
