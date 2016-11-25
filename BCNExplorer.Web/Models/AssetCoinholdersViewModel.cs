using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Core.AssetBlockChanges.Mongo;

namespace BCNExplorer.Web.Models
{
    public class AssetCoinholdersViewModel
    {
        public AssetViewModel Asset { get; set; }

        public IEnumerable<BalanceAddressSummary> AddressSummaries { get; set; }
        public double Total { get; set; }
        public int AtBlockHeight { get; set; }
        
        public BlockPagination Pagination { get; set; }
        
        public static AssetCoinholdersViewModel Create(AssetViewModel asset, BalanceSummary balanceSummary)
        {
            var total = balanceSummary.AddressSummaries.Sum(p => p.Balance);
            
            return new AssetCoinholdersViewModel
            {
                Asset = asset,
                AddressSummaries = balanceSummary.AddressSummaries.Select(p => BalanceAddressSummary.Create(p, total, asset.Divisibility)).Where(p=>p.Balance != 0 || p.ChangeAtBlock != 0),
                Total = BitcoinUtils.CalculateColoredAssetQuantity(total, asset.Divisibility),
                Pagination = BlockPagination.Create(balanceSummary.ChangedAtHeights, balanceSummary.AtBlockHeight)

            };
        }

        public class BalanceAddressSummary
        {
            public string Address { get; set; }
            public double Balance { get; set; }
            public double ChangeAtBlock { get; set; }
            public double BalancePercenage { get; set; }

            public static BalanceAddressSummary Create(BalanceSummary.BalanceAddressSummary summary, double total, int divisibility)
            {
                var coloredTotal = BitcoinUtils.CalculateColoredAssetQuantity(total, divisibility);
                var coloredSummaryPerAddress = BitcoinUtils.CalculateColoredAssetQuantity(summary.Balance, divisibility);
                var coloredChangeAtBlock = BitcoinUtils.CalculateColoredAssetQuantity(summary.ChangeAtBlock,
                    divisibility);

                return new BalanceAddressSummary
                {
                    Address = summary.Address,
                    Balance = coloredSummaryPerAddress,
                    BalancePercenage = Math.Round((coloredSummaryPerAddress / coloredTotal) * 100),
                    ChangeAtBlock = coloredChangeAtBlock
                };
            }
        }
        
        public class BlockPagination
        {
            public int[] ChangedAtHeights { get; set; }
            public bool ShowCurrentBlock { get; set; }

            public int AtBlock;

            public int Start => ChangedAtHeights.FirstOrDefault();
            public int Last => ChangedAtHeights.LastOrDefault();

            public bool ShowNext => Last > AtBlock;
            public bool ShowPrev => Start < AtBlock;

            public int NextBlock => ChangedAtHeights.Where(p => p > AtBlock).First();
            public int PrevBlock => ChangedAtHeights.Where(p => p < AtBlock).Last();

            public static BlockPagination Create(IEnumerable<int> changedAtHeights, int? atBlock)
            {
                var ordered = changedAtHeights.OrderBy(p => p).ToArray();
                return new BlockPagination
                {
                    ChangedAtHeights = ordered,
                    AtBlock = atBlock ?? ordered.LastOrDefault(),
                    ShowCurrentBlock = atBlock != null
                };
            }
        }
    }
}