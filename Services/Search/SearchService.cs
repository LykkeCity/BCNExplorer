using System.Threading.Tasks;
using Core.Asset;
using Core.Channel;
using Core.SearchService;
using Core.Settings;
using Providers.Helpers;
using Providers.Providers.Ninja;

namespace Services.Search
{
    public class SearchService:ISearchService
    {
        private readonly NinjaSearchProvider _ninjaSearchProvider;
        private readonly IAssetService _assetProvider;
        private readonly BaseSettings _baseSettings;
        private readonly IChannelService _channelService;

        public SearchService(NinjaSearchProvider ninjaSearchProvider, 
            IAssetService assetProvider, 
            BaseSettings baseSettings, 
            IChannelService channelService)
        {
            _ninjaSearchProvider = ninjaSearchProvider;
            _assetProvider = assetProvider;
            _baseSettings = baseSettings;
            _channelService = channelService;
        }

        public async Task<SearchResultType?> GetTypeAsync(string id)
        {
            if (BitcoinAddressHelper.IsAddress(id, _baseSettings.UsedNetwork()))
            {
                return SearchResultType.Address;
            }

            if (await _channelService.OffchainTransactionExistsAsync(id))
            {
                return SearchResultType.OffchainTransaction;
            }

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
