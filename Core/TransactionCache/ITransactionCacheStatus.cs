using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.TransactionCache
{
    public interface ITransactionCacheStatus
    {
        int BlockHeight { get; }
        string Address { get; }
        bool FullLoaded { get; }
    }

    public interface ITransactionCacheStatusRepository
    {
        Task<ITransactionCacheStatus> GetAsync(string address);
        Task SetAsync(string address, int blockHeight, bool fullLoaded);
    }
}
