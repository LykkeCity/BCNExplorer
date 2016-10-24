using System.Collections.Generic;
using System.Linq;
using NBitcoin;
using NBitcoin.Indexer;

namespace Providers.Helpers
{
    public static class OrderedBalanceHelper
    {
        public static IEnumerable<ColoredChange> GetColoredChanges(
            this OrderedBalanceChange orderedBalanceChange, Network network)
        {
            foreach (var coin in orderedBalanceChange.ReceivedCoins.OfType<ColoredCoin>())
            {
                yield return new ColoredChange
                {
                    AssetId = coin.AssetId.GetWif(network).ToString(),
                    Quantity = coin.Asset.Quantity,
                    BlockHash = orderedBalanceChange.BlockId.ToString()
                };
            }
            foreach (var coin in orderedBalanceChange.SpentCoins.OfType<ColoredCoin>())
            {
                yield return new ColoredChange
                {
                    AssetId = coin.AssetId.GetWif(network).ToString(),
                    Quantity = (-1) * coin.Asset.Quantity,
                    BlockHash = orderedBalanceChange.BlockId.ToString()
                };
            }
        }
    }

    public class ColoredChange
    {
        public string AssetId { get; set; }
        public long Quantity { get; set; }
        public long BlockHeight { get; set; }
        public string BlockHash { get; set; }
    }
}
