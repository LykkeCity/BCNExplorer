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

    public interface IAssetBalanceChangesRepository
    {
        Task AddAsync(IEnumerable<IBalanceChanges> balanceChanges);
    }
}
