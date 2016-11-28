using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Asset;
using Providers.Contracts.Asset;
using Providers.TransportTypes.Asset;

namespace Providers.Providers.Asset
{
    public class AssetIndexer
    {
        public static Dictionary<string, IAsset> IndexAssets(IEnumerable<IAsset> assets)
        {
            var result = new Dictionary<string, IAsset>(StringComparer.OrdinalIgnoreCase);

            foreach (var asset in assets)
            {
                result[asset.Name] = asset;
                result[asset.NameShort] = asset;

                foreach (var assetId in asset.AssetIds ?? Enumerable.Empty<string>())
                {
                    result[assetId] = asset;
                }
            }

            return result;
        }

        public static Dictionary<string, IAsset> IndexAssets(IEnumerable<AssetContract> assets)
        {
            return IndexAssets(assets.Select(AssetDefinition.Create));
        }
    }
}
