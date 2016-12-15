using System;
using System.Collections.Generic;
using System.Linq;
using Core.Asset;

namespace Providers.Helpers
{
    public static class AssetScoreHelper
    {

        public static double CalculateAssetScore(IAssetDefinition assetDefinition, IAssetCoinholdersIndex index,
            IEnumerable<IAssetCoinholdersIndex> allIndexes)
        {
            var isVerified = (assetDefinition?.IsVerified??false) ? 0 : 1;

            return Weight(Coef.IsVerified) * isVerified
                + Weight(Coef.LastMonthTxCount) * Rank(index.LastMonthTransactionCount, allIndexes.Select(p => p.LastMonthTransactionCount))
                + Weight(Coef.TotalTransactionsCount) * Rank(index.TransactionsCount, allIndexes.Select(p => p.TransactionsCount))
                + Weight(Coef.CoinholdersCount) * Rank(index.CoinholdersCount, allIndexes.Select(p => p.CoinholdersCount))
                + Weight(Coef.TotalQuantity) * Rank(index.TotalQuantity, allIndexes.Select(p => p.TotalQuantity))
                + Weight(Coef.LastTxDateDaysPast) * (index.LastTxDateDaysPast() != null ? Rank(index.LastTxDateDaysPast().Value, allIndexes.Select(p => p.LastTxDateDaysPast() ?? 0)) : 1)
                + Weight(Coef.TopCoinholderShare) * Rank(index.TopCoinholderShare, allIndexes.Select(p => p.TopCoinholderShare))
                + Weight(Coef.HerfindalShareIndex) * Rank(index.HerfindalShareIndex, allIndexes.Select(p => p.HerfindalShareIndex));
        }

        private static double Rank(double value, IEnumerable<double> allValues)
        {
            return Rank(value, allValues.DefaultIfEmpty().Min(), allValues.DefaultIfEmpty().Max());
        }

        private static double Rank(int value, IEnumerable<int> allValues)
        {
            return Rank(value, allValues.DefaultIfEmpty().Min(), allValues.DefaultIfEmpty().Max());
        }


        private static double Rank(double value, double min, double max)
        {
            return (value - min) / (max - min);
        }

        #region Weights

        enum Coef
        {
            IsVerified,
            LastMonthTxCount,
            TotalTransactionsCount,
            CoinholdersCount,
            TotalQuantity,
            LastTxDateDaysPast,
            TopCoinholderShare,
            HerfindalShareIndex
        }

        private static IDictionary<Coef, double> _weights = new Dictionary<Coef, double>
        {
            {Coef.IsVerified, 5},
            {Coef.LastMonthTxCount, 8},
            {Coef.TotalTransactionsCount, 9},
            {Coef.CoinholdersCount, 10},
            {Coef.TotalQuantity, 0},
            {Coef.LastTxDateDaysPast, 4},
            {Coef.TopCoinholderShare, 3},
            {Coef.HerfindalShareIndex, 7}
        };


        private static double Weight(Coef flag)
        {
            return _weights[flag] / _weights.Values.Sum();
        }
        
        #endregion
    }

    public static class AssetIndexerHelper
    {
        public static int? LastTxDateDaysPast(this IAssetCoinholdersIndex index)
        {
            if (index.LastTxDate != null)
            {
                return (DateTime.Now - index.LastTxDate.Value).Days;
            }

            return null;
        }
    }
}
