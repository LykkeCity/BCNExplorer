using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.AssetBlockChanges.Mongo
{
    public interface IBalanceChanges
    {
        string AssetId { get; }
        long Quantity { get; }
        string BlockHash { get;  }
        int BlockHeight { get; }
        string TransactionHash { get;  }
    }

    public class AssetBalanceChanges: IBalanceChanges
    {
        public string AssetId { get; set; }
        public long Quantity { get; set; }
        public string BlockHash { get; set; }
        public int BlockHeight { get; set; }
        public string TransactionHash { get; set; }

        public static AssetBalanceChanges Create(string assetId, long quantity, string blockHash, int blockHeight,
            string transactionHash)
        {
            return new AssetBalanceChanges
            {
                AssetId = assetId,
                Quantity = quantity,
                BlockHash = blockHash,
                BlockHeight = blockHeight,
                TransactionHash = transactionHash
            };
        }
    }

    public interface IBalanceSummary
    {

        IEnumerable<string> AssetIds { get; }
        IEnumerable<IBalanceAddressSummary> AddressSummaries { get;  }
        IEnumerable<int> ChangedAtHeights { get;  }
        int? AtBlockHeight { get;  }
    }

    public interface IBalanceAddressSummary
    {
        string Address { get; }
        double Balance { get; }
        double ChangeAtBlock { get; }
    }

    public interface IAssetBalanceChangesRepository
    {
        Task AddAsync(string coloredAddress, IEnumerable<IBalanceChanges> balanceChanges);
        Task<IBalanceSummary> GetSummaryAsync(params string[] assetIds);
        Task<IBalanceSummary> GetSummaryAsync(int? atBlock, params string[] assetIds);
        Task<int> GetLastParsedBlockHeightAsync();
    }
}
