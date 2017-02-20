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
        public static Dictionary<string, IAssetDefinition> IndexAssetsDefinitions(IEnumerable<IAssetDefinition> assets)
        {
            var result = new Dictionary<string, IAssetDefinition>(StringComparer.OrdinalIgnoreCase);

            foreach (var asset in assets.Where(p=>p.AssetIds.Any() && p.AssetIds.All(x=>x!=null)))
            {
                if (!string.IsNullOrEmpty(asset.Name))
                {
                    result[asset.Name] = asset;
                }

                if (!string.IsNullOrEmpty(asset.NameShort))
                {
                    result[asset.NameShort] = asset;
                }

                foreach (var assetId in (asset.AssetIds ?? Enumerable.Empty<string>()).Where(p=>!string.IsNullOrEmpty(p)))
                {
                    result[assetId] = asset;
                }
            }

            return result;
        }

        public static Dictionary<string, IAssetDefinition> IndexAssetsDefinitions(IEnumerable<AssetContract> assets)
        {
            return IndexAssetsDefinitions(assets.Select(AssetDefinition.Create));
        }

        public static Dictionary<string, IAssetCoinholdersIndex> IndexAssetCoinholders(IEnumerable<IAssetCoinholdersIndex> assets)
        {
            var result = new Dictionary<string, IAssetCoinholdersIndex>(StringComparer.OrdinalIgnoreCase);

            foreach (var asset in assets)
            {
                foreach (var assetId in (asset.AssetIds ?? Enumerable.Empty<string>()).Where(p => !string.IsNullOrEmpty(p)))
                {
                    result[assetId] = asset;
                }
            }

            return result;
        }

        public static Dictionary<string, IAssetScore> IndexAssetScores(IEnumerable<IAssetScore> assets)
        {
            var result = new Dictionary<string, IAssetScore>(StringComparer.OrdinalIgnoreCase);

            foreach (var asset in assets)
            {

                foreach (var assetId in ((asset.AssetIds ?? Enumerable.Empty<string>()).Where(p => !string.IsNullOrEmpty(p))))
                {
                    result[assetId] = asset;
                }
            }

            return result;
        }
    }
}
