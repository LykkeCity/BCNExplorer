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
    public interface IAssetBalanceChangesRepository
    {
        Task AddAsync(IEnumerable<BalanceChange> balanceChanges);
    }
}
