using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;
using NBitcoin.Indexer;
using NBitcoin.OpenAsset;

namespace Providers.Helpers
{
    public static class OrderedBalanceHelper
    {
        public static IEnumerable<ColoredChange> GetColoredChanges(
            this OrderedBalanceChange orderedBalanceChange, Network network)
        {
           
            var transfers = orderedBalanceChange?.ColoredTransaction?.Transfers??Enumerable.Empty<ColoredEntry>();
            foreach (var coloredEntry in transfers)
            {
                
                yield return new ColoredChange
                {
                    AssetId = coloredEntry.Asset.Id.GetWif(network).ToString(),
                    Quantity = (-1) * coloredEntry.Asset.Quantity
                };
            }

            var issuances = orderedBalanceChange?.ColoredTransaction?.Issuances ?? Enumerable.Empty<ColoredEntry>();
            foreach (var coloredEntry in issuances)
            {

                yield return new ColoredChange
                {
                    AssetId = coloredEntry.Asset.Id.GetWif(network).ToString(),
                    Quantity = coloredEntry.Asset.Quantity
                };
            }
        }

        public static IEnumerable<OrderedBalanceChange> GetConfirmed(this IEnumerable<OrderedBalanceChange> )
        {
            
        }
    }

    public class ColoredChange
    {
        public string AssetId { get; set; }
        public long Quantity { get; set; }
    }
}
