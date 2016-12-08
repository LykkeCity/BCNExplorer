using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.AssetBlockChanges.Mongo;

namespace Core.Asset
{
    public class AssetCoinholdersIndex: IAssetCoinholdersIndex
    {
        private const int CoinholdersToStore = 20;
        public IEnumerable<string> AssetIds { get; set; }
        public IDictionary<string, double> TopCoinholdersBalanceAddressDictionary { get; set; }
        public double Spread { get; }
        public IEnumerable<int> ChangedAtBlockHeights { get; set; }
        public int CoinholdersCount { get; set; }
        public double TotalQuantity { get; set; }

        public static AssetCoinholdersIndex Create(IBalanceSummary balanceSummary)
        {
            var addressDic = balanceSummary.AddressSummaries.GroupBy(p => p.Address).ToDictionary(p => p.Key, p => p.Sum(x => x.Balance));
            return new AssetCoinholdersIndex
            {
                AssetIds = balanceSummary.AssetIds,
                TopCoinholdersBalanceAddressDictionary = addressDic.OrderByDescending(p => p.Value).Take(CoinholdersToStore).ToDictionary(p=>p.Key, p=>p.Value),
                ChangedAtBlockHeights = balanceSummary.ChangedAtHeights,
                CoinholdersCount = addressDic.Count,
                TotalQuantity = addressDic.Sum(p=>p.Value)
            };
        }
    }

    public interface IAssetCoinholdersIndex
    {
        IEnumerable<string> AssetIds { get; }
        IDictionary<string, double> TopCoinholdersBalanceAddressDictionary { get;}
        double Spread { get; }
        IEnumerable<int> ChangedAtBlockHeights { get; } 
        int CoinholdersCount { get; }
        double TotalQuantity { get; }
    }

    public interface IAssetCoinholdersIndexRepository
    {
        Task InserOrReplaceAsync(IAssetCoinholdersIndex index);
        Task<IEnumerable<IAssetCoinholdersIndex>> GetAllAsync();
    }
}
