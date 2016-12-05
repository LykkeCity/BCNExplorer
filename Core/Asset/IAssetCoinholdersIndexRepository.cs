using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.AssetBlockChanges.Mongo;

namespace Core.Asset
{
    public class AssetCoinholdersIndex: IAssetCoinholdersIndex
    {
        public IEnumerable<string> AssetIds { get; set; }
        public IDictionary<string, double> BalanceAddressDictionary { get; set; }
        public double Spread { get; }

        public static AssetCoinholdersIndex Create(IBalanceSummary balanceSummary)
        {
            return new AssetCoinholdersIndex
            {
                AssetIds = balanceSummary.AssetIds,
                BalanceAddressDictionary = balanceSummary.AddressSummaries.GroupBy(p=>p.Address).ToDictionary(p=>p.Key, p=>p.Sum(x=>x.Balance))
            };
        }
    }

    public interface IAssetCoinholdersIndex
    {
        IEnumerable<string> AssetIds { get; }
        IDictionary<string, double> BalanceAddressDictionary { get;}
        double Spread { get; }
    }

    public interface IAssetCoinholdersIndexRepository
    {
        Task InserOrReplaceAsync(IAssetCoinholdersIndex index);
        Task<IEnumerable<IAssetCoinholdersIndex>> GetAllAsync();
    }
}
