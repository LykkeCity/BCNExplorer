using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Transaction
{
    public interface ICachedTransactionService
    {
        Task<ITransaction> GetAsync(string id);
        Task<IEnumerable<ITransaction>> GetAsync(IEnumerable<string> ids);
    }
}
