using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Common.CSV;
using Core.AssetBlockChanges.Mongo;
using Core.Block;

namespace BCNExplorer.Web.Models
{
    public class AssetCoinholdersViewModel
    {
        public AssetViewModel Asset { get; set; }

        public IEnumerable<BalanceAddressSummary> AddressSummaries { get; set; }
        public double Total { get; set; }
        public int CoinholdersCount { get; set; }
        
        public BlockPagination Pagination { get; set; }

        public DateTime? AtBlockDateTime { get; set; }
         
        public static AssetCoinholdersViewModel Create(AssetViewModel asset,
            IBalanceSummary balanceSummary, 
            int? atBlockHeight,
            IDictionary<string, double> addressChanges,
            IEnumerable<IBalanceBlock> blocksWithChanges,
            IBlockHeader currentBlock,
            IBlockHeader atBlockInfo)
        {
            var total = balanceSummary.AddressSummaries.Sum(p => p.Balance);
            var addressSummaries = balanceSummary.AddressSummaries
                .Select(
                    p =>
                        BalanceAddressSummary.Create(p, total, asset.Divisibility,
                            addressChanges.ContainsKey(p.Address) ? addressChanges[p.Address] : 0))
                .Where(p => p.Balance != 0 || p.ChangeAtBlock != 0)
                .OrderByDescending(p => p.Balance)
                .ToList();
            return new AssetCoinholdersViewModel
            {
                Asset = asset,
                AddressSummaries = addressSummaries,
                Total = BitcoinUtils.CalculateColoredAssetQuantity(total, asset.Divisibility),
                Pagination = BlockPagination.Create(blocksWithChanges.Select(p=>p.Height), atBlockHeight??currentBlock?.Height, currentBlock),
                CoinholdersCount = addressSummaries.Count,
                AtBlockDateTime = (atBlockInfo ?? currentBlock)?.Time.ToUniversalTime()
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
            public IEnumerable<int> ChangedAtHeights { get; set; }
            public bool ShowCurrentBlock { get; set; }

            public int AtBlock;

            public int First => ChangedAtHeights.FirstOrDefault();
            public int Last => ChangedAtHeights.LastOrDefault();

            public bool ShowPrev => First < AtBlock;
            public int PrevBlock => ChangedAtHeights.LastOrDefault(p => p < AtBlock);

            public bool ShowNext => Last > AtBlock;
            public int NextBlock => ChangedAtHeights.FirstOrDefault(p => p > AtBlock);


            public static BlockPagination Create(IEnumerable<int> changedAtHeights, int? atBlock, IBlockHeader currentBlock)
            {
                var ordered = changedAtHeights.OrderBy(p => p).ToList();
                var at = atBlock ?? ordered.LastOrDefault();

                if (currentBlock != null)
                {
                    ordered.Add(currentBlock.Height);
                }


                var result = new BlockPagination
                {
                    ChangedAtHeights = ordered.Distinct().ToList(),
                    AtBlock = at,
                    ShowCurrentBlock = atBlock != null
                };

                return result;
            }
        }

        public byte[] ToCsv()
        {
            var result = new StringBuilder();
            result.AppendLine("Address,Amount,Ownership");

            foreach (var addressSum in AddressSummaries)
            {
                result.AppendLine($"{CsvHelper.Escape(addressSum.Address)},{CsvHelper.Escape(addressSum.Balance.ToStringBtcFormat())} {CsvHelper.Escape(Asset.NameShort)},{CsvHelper.Escape(addressSum.BalancePercentageDescription)} %");
            }

            result.AppendLine($"Total,{CsvHelper.Escape(Total.ToStringBtcFormat())} {CsvHelper.Escape(Asset.NameShort)}");

            return new UTF8Encoding().GetBytes(result.ToString());
        }
        
    }
}