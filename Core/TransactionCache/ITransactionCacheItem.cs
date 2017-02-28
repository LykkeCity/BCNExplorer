using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.TransactionCache
{
    public interface ITransactionCacheItem
    {
        string TransactionId { get; }
        bool IsReceived { get; }
        string BlockHash { get; }
        int? BlockHeight { get; }
        string Address { get; }
    }

    public interface ITransactionCacheRepository
    {
        Task InsertOrReplaceAsync(IEnumerable<ITransactionCacheItem> transactions);
        Task<IEnumerable<ITransactionCacheItem>> GetAsync(string address);
        Task<ITransactionCacheItem> GetLastCachedTransaction(string address);
    }
}
