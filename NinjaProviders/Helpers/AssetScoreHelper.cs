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
            var result =  Weight(Coef.IsVerified) * isVerified
                + Weight(Coef.LastMonthTxCount) * Calc(index.LastMonthTransactionCount, allIndexes.Select(p => p.LastMonthTransactionCount))
                + Weight(Coef.TotalTransactionsCount) * Calc(index.TransactionsCount, allIndexes.Select(p => p.TransactionsCount))
                + Weight(Coef.CoinholdersCount) * Calc(index.CoinholdersCount, allIndexes.Select(p => p.CoinholdersCount))
                + Weight(Coef.TotalQuantity) * Calc(index.TotalQuantity, allIndexes.Select(p => p.TotalQuantity))
                + Weight(Coef.LastTxDateDaysPast) * (index.LastTxDateDaysPast() != null ? Calc(index.LastTxDateDaysPast().Value, allIndexes.Select(p => p.LastTxDateDaysPast() ?? 0)) : 1)
                + Weight(Coef.TopCoinholderShare) * index.TopCoinholderShare
                + Weight(Coef.HerfindalShareIndex) * index.HerfindalShareIndex;

            return Math.Round(result, 4);
        }


        private static double Calc(double value, IEnumerable<double> allValues)
        {
            var rankArray = allValues.Distinct().Select(p => Rank(p, allValues)).ToList();
            return Normalize(Rank(value, allValues), rankArray.Min(), rankArray.Max());
        }

        private static double Calc(int value, IEnumerable<int> allValues)
        {
            var rankArray = allValues.Distinct().Select(p => Rank(p, allValues)).ToList();
            return Normalize(Rank(value, allValues), rankArray.Min(), rankArray.Max());
        }

        private static double Rank(double value, IEnumerable<double> allValues)
        {
            var unique = allValues.Distinct().OrderByDescending(p=>p).ToList();

            return unique.IndexOf(value) + 1;
        }

        private static double Rank(int value, IEnumerable<int> allValues)
        {
            return Rank(value, allValues.Select(p => (double) p));
        }

        /// <summary>
        /// To 0-1
        /// </summary>
        private static double Normalize(double value, double min, double max)
        {
            return (value - min) / (max - min);
        }


        private static double Normalize(double value, IEnumerable<double> allValues)
        {
            return Normalize(value, allValues.DefaultIfEmpty().Min(), allValues.DefaultIfEmpty().Max());
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
            {Coef.IsVerified, 10},
            {Coef.LastMonthTxCount, 9},
            {Coef.TotalTransactionsCount, 3},
            {Coef.CoinholdersCount, 2},
            {Coef.TotalQuantity, 0},
            {Coef.LastTxDateDaysPast, 10},
            {Coef.TopCoinholderShare, 1},
            {Coef.HerfindalShareIndex, 1}
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
