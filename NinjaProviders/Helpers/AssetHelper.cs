using System;
using NBitcoin;
using NBitcoin.DataEncoders;
using NBitcoin.OpenAsset;

namespace Providers.Helpers
{
    public static class AssetHelper
    {
        public static Uri TryGetAssetDefinitionUrl(string hex)
        {
            try
            {
                var tx = Transaction.Parse(hex);
                
                var coloredMarker = tx.GetColoredMarker();

                return coloredMarker.GetMetadataUrl();
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
