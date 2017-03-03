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
        public static Dictionary<string, IAssetDefinition> IndexAssetsDefinitions(IEnumerable<IAssetDefinition> assets, IEnumerable<IAssetImage> assetImages)
        {
            var result = new Dictionary<string, IAssetDefinition>(StringComparer.OrdinalIgnoreCase);
            var imageDictionary = IndexAssetsImages(assetImages);
            
            foreach (var asset in assets.Where(p => p.AssetIds.Any() && p.AssetIds.All(x=>x!=null)))
            {

                #region getting Cached images 

                asset.ImageUrl = null;
                asset.IconUrl = null;

                foreach (var assetId in asset.AssetIds)
                {
                    if (imageDictionary.ContainsKey(assetId))
                    {
                        var image = imageDictionary[assetId];
                        asset.IconUrl = image.IconUrl;
                        asset.ImageUrl = image.ImageUrl;

                        break;
                    }
                }

                #endregion

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

        public static Dictionary<string, IAssetImage> IndexAssetsImages(IEnumerable<IAssetImage> assetImages)
        {
            var result = new Dictionary<string, IAssetImage>();
            foreach (var assetImage in assetImages ?? Enumerable.Empty<IAssetImage>())
            {
                foreach (var assetId in (assetImage.AssetIds??Enumerable.Empty<string>()). Where(p => !string.IsNullOrEmpty(p)))
                {
                    result[assetId] = assetImage;
                }
            }

            return result;

            return result;
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
