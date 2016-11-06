using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.AssetBlockChanges
{
    public interface IBalanceChange
    {
        long Id { get; }
        string AssetId { get; }
        double Change { get; }
        string TransactionHash { get; }
    }

    public class BalanceChange: IBalanceChange
    {
        public long Id { get; set; }
        public string AssetId { get; set; }
        public double Change { get; set; }
        public string TransactionHash { get; set; }
    }

    public interface IBalanceChangesRepository
    {
        Task AddAsync(params IBalanceChange[] balanceChanges);
    }
}
