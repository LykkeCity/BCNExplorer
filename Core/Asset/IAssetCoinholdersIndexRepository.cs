using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Core.AssetBlockChanges.Mongo;

namespace Core.Asset
{
    public class AssetCoinholdersIndex: IAssetCoinholdersIndex
    {
        public IEnumerable<string> AssetIds { get; set; }
        public int CoinholdersCount { get; set; }
        public double TotalQuantity { get; set; }
        public double TopCoinholderShare { get; set; }
        public double HerfindalShareIndex { get; set; }
        public DateTime? LastTxDate { get; set; }
        public int TransactionsCount { get; set; }
        public int LastMonthTransactionCount { get; set; }

        public static AssetCoinholdersIndex Create(IBalanceSummary balanceSummary, IEnumerable<IBalanceBlock> blocksWithChanges, int transactionCount, int lastMonthTransactionCount, DateTime? lastTxDate)
        {
            var addressDic = balanceSummary.AddressSummaries.GroupBy(p => p.Address)
                .Where(p => p.Sum(x=>x.Balance) != 0)
                .ToDictionary(p => p.Key, p => p.Sum(x => x.Balance));
            var totalQuantity = addressDic.Sum(p => p.Value);
            return new AssetCoinholdersIndex
            {
                AssetIds = balanceSummary.AssetIds,
                CoinholdersCount = addressDic.Count,
                TotalQuantity = totalQuantity,
                HerfindalShareIndex = HerfindahlIndex.Calculate(addressDic.Values.Select(p => HerfindahlIndex.CalculateShare(p, totalQuantity))),
                TopCoinholderShare = HerfindahlIndex.CalculateShare(addressDic.Values.DefaultIfEmpty().Max(), totalQuantity),
                TransactionsCount = transactionCount,
                LastTxDate = lastTxDate,
                LastMonthTransactionCount = lastMonthTransactionCount
            };
        }
    }

    public interface IAssetCoinholdersIndex
    {
        IEnumerable<string> AssetIds { get; }
        int CoinholdersCount { get; }
        double TotalQuantity { get; }
        double TopCoinholderShare { get; }
        double HerfindalShareIndex { get; }
        DateTime? LastTxDate { get; }
        int TransactionsCount { get; }
        int LastMonthTransactionCount { get; }
    }


    public interface IAssetCoinholdersIndexRepository
    {
        Task InserOrReplaceAsync(IAssetCoinholdersIndex index);
        Task<IEnumerable<IAssetCoinholdersIndex>> GetAllAsync();
    }
}
