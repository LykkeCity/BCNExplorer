using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Providers.Providers.Lykke;
using Providers.Providers.Ninja;
using Providers.TransportTypes.Ninja;

namespace Providers.Providers.Common
{
    public class SearchProvider
    {
        private readonly NinjaSearchProvider _ninjaSearchProvider;
        private readonly LykkeAssetProvider _lykkeAssetProvider;

        public SearchProvider(NinjaSearchProvider ninjaSearchProvider, LykkeAssetProvider lykkeAssetProvider)
        {
            _ninjaSearchProvider = ninjaSearchProvider;
            _lykkeAssetProvider = lykkeAssetProvider;
        }

        public async Task<NinjaType?> GetTypeAsync(string id)
        {
            var result = await _ninjaSearchProvider.GetTypeAsync(id);

            if (result == null)
            {
                var asset = await _lykkeAssetProvider.GetAssetAsync(id);
                result = asset != null ? (NinjaType?)NinjaType.Asset : null;
            }

            return result;
        }
    }
}
