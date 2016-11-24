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

    public class BalanceSummary
    {
        public string AssetId { get; set; }
        public IEnumerable<BalanceAddressSummary> AddressSummaries { get; set; }
        public IEnumerable<int> ChangedAtHeights { get; set; }
        public int? AtBlockHeight { get; set; }
        public class BalanceAddressSummary
        {
            public string Address { get; set; }
            public double Balance { get; set; }
            public double ChangeAtBlock { get; set; }
        }
    }

    public interface IAssetBalanceChangesRepository
    {
        Task AddAsync(string coloredAddress, IEnumerable<IBalanceChanges> balanceChanges);
        Task<BalanceSummary> GetSummaryAsync(params string[] assetIds);
        Task<BalanceSummary> GetSummaryAsync(int? atBlock, params string[] assetIds);
        Task<int> GetLastParsedBlockHeightAsync();
    }
}
