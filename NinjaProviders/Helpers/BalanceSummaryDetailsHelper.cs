using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;
using NBitcoin.Indexer;
using NBitcoin.OpenAsset;
using QBitNinja.Client.Models;

namespace Providers.Helpers
{
    public static class BalanceSummaryDetailsHelper
    {
        public static BalanceSummaryDetails CreateFrom(IEnumerable<OrderedBalanceChange> changes, Network network, bool colored)
        {
            var details = new BalanceSummaryDetails
            {
                Amount = CalculateAmount(changes),
                TransactionCount = changes.Count(),
                Received = changes.Select(_ => _.Amount < Money.Zero ? Money.Zero : _.Amount).Sum(),
            };

            if (colored)
            {
                Dictionary<AssetId, AssetBalanceSummaryDetails> coloredDetails = new Dictionary<AssetId, AssetBalanceSummaryDetails>();
                foreach (var change in changes)
                {
                    foreach (var coin in change.ReceivedCoins.OfType<ColoredCoin>())
                    {
                        AssetBalanceSummaryDetails coloredDetail = null;
                        if (!coloredDetails.TryGetValue(coin.AssetId, out coloredDetail))
                        {
                            coloredDetail = new AssetBalanceSummaryDetails();
                            coloredDetail.Asset = coin.AssetId.GetWif(network);
                            coloredDetails.Add(coin.AssetId, coloredDetail);
                        }
                        coloredDetail.Quantity += (long)coin.Asset.Quantity;
                        coloredDetail.Received += (long)coin.Asset.Quantity;
                    }
                    foreach (var coin in change.SpentCoins.OfType<ColoredCoin>())
                    {
                        AssetBalanceSummaryDetails coloredDetail = null;
                        if (!coloredDetails.TryGetValue(coin.AssetId, out coloredDetail))
                        {
                            coloredDetail = new AssetBalanceSummaryDetails();
                            coloredDetail.Asset = coin.AssetId.GetWif(network);
                            coloredDetails.Add(coin.AssetId, coloredDetail);
                        }
                        coloredDetail.Quantity -= (long)coin.Asset.Quantity;
                    }
                    
                }
                details.Assets = coloredDetails.Values.ToArray();
            }
            return details;
        }

        public static Money CalculateAmount(IEnumerable<OrderedBalanceChange> changes)
        {
            return changes.SelectMany(c => c.ReceivedCoins.OfType<Coin>()).Select(c => c.Amount).Sum()
                -
                changes.SelectMany(c => c.SpentCoins.OfType<Coin>()).Select(c => c.Amount).Sum();
        }

    }
}
