using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.TransactionCache
{
    public interface IAddressTransaction
    {
        string TransactionId { get; }
        bool IsReceived { get; }
    }

    public interface ITransactionCacheItemRepository
    {
        Task SetAsync(string address, IEnumerable<IAddressTransaction> transactions);
        Task<IEnumerable<IAddressTransaction>> GetAsync(string address);
    }
}
