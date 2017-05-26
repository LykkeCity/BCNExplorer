using System.Threading.Tasks;

namespace Core.Transaction
{
    public interface ICachedTransactionService
    {
        Task<ITransaction> GetAsync(string id);
    }
}
