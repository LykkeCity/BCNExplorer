using System;
using NBitcoin;
using NBitcoin.OpenAsset;

namespace Providers.Helpers
{
    public static class AssetHelper
    {
        public static Uri TryGetAssetDefinitionUrl(string hex)
        {
            try
            {
                var tx = new Transaction(hex);
                var color = ColorMarker.TryParse(tx);

                return color.GetMetadataUrl();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
