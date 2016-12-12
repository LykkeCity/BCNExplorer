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
        
        public static AssetCoinholdersViewModel Create(AssetViewModel asset,
            IBalanceSummary balanceSummary, 
            int? atBlockHeight,
            IDictionary<string, double> addressChanges,
            IEnumerable<IBalanceBlock> blocksWithChanges)
        {
            var total = balanceSummary.AddressSummaries.Sum(p => p.Balance);
            
            return new AssetCoinholdersViewModel
            {
                Asset = asset,
                AddressSummaries = balanceSummary.AddressSummaries
                    .Select(p => BalanceAddressSummary.Create(p, total, asset.Divisibility, addressChanges.ContainsKey(p.Address) ? addressChanges[p.Address] : 0))
                    .Where(p=>p.Balance != 0 || p.ChangeAtBlock != 0)
                    .OrderByDescending(p => p.Balance),
                Total = BitcoinUtils.CalculateColoredAssetQuantity(total, asset.Divisibility),
                Pagination = BlockPagination.Create(blocksWithChanges.Select(p=>p.Height), atBlockHeight)

            };
        }

        public class BalanceAddressSummary
        {
            public string Address { get; set; }
            public double Balance { get; set; }
            public double ChangeAtBlock { get; set; }
            public double BalancePercenage { get; set; }

            public string BalancePercentageDescription => BalancePercenage > 0.01 ? BalancePercenage.ToString("F2") : "<0.01";

            public static BalanceAddressSummary Create(IBalanceAddressSummary summary, double total, int divisibility, double changesAtBlock)
            {
                var coloredTotal = BitcoinUtils.CalculateColoredAssetQuantity(total, divisibility);
                var coloredSummaryPerAddress = BitcoinUtils.CalculateColoredAssetQuantity(summary.Balance, divisibility);
                var coloredChangeAtBlock = BitcoinUtils.CalculateColoredAssetQuantity(changesAtBlock,
                    divisibility);

                return new BalanceAddressSummary
                {
                    Address = summary.Address,
                    Balance = coloredSummaryPerAddress,
                    BalancePercenage = Math.Round((coloredSummaryPerAddress / coloredTotal) * 100, 2),
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