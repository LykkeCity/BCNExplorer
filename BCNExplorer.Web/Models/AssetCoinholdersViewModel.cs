using System.Collections.Generic;
using System.Linq;
using Common;
using Core.AssetBlockChanges;

namespace BCNExplorer.Web.Models
{
    public class AssetCoinholdersViewModel
    {
        public AssetViewModel Asset { get; set; }

        public IEnumerable<BalanceAddressSummary> AddressSummaries { get; set; }
        public double Total { get; set; }

        public static AssetCoinholdersViewModel Create(AssetViewModel asset, BalanceSummary balanceSummary)
        {
            var total = balanceSummary.AddressSummaries.Sum(p => p.Balance);
            return new AssetCoinholdersViewModel
            {
                Asset = asset,
                AddressSummaries = balanceSummary.AddressSummaries.Select(p => BalanceAddressSummary.Create(p, total, asset.Divisibility)),
                Total = BitcoinUtils.CalculateColoredAssetQuantity(total, asset.Divisibility)
            };
        }

        public class BalanceAddressSummary
        {
            public string Address { get; set; }
            public double Balance { get; set; }
            public double BalancePercenage { get; set; }

            public static BalanceAddressSummary Create(BalanceSummary.BalanceAddressSummary summary, double total, int divisibility)
            {
                var coloredTotal = BitcoinUtils.CalculateColoredAssetQuantity(total, divisibility);
                var coloredSummaryPerAddress = BitcoinUtils.CalculateColoredAssetQuantity(summary.Balance, divisibility);

                return new BalanceAddressSummary
                {
                    Address = summary.Address,
                    Balance = coloredSummaryPerAddress,
                    BalancePercenage = (coloredSummaryPerAddress / coloredTotal) * 100
                };
            }
        }
    }
}