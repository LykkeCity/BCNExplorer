using System.Threading.Tasks;

namespace Core.AssetBlockChanges
{
    public interface ITransaction
    {
        string Hash { get; }
        string BlockHash { get; }
    }

    public class Transaction:ITransaction
    {
        public string Hash { get; set; }
        public string BlockHash { get; set; }
    }

    public interface ITransactionRepository
    {
        Task AddAsync(ITransaction[] transactions);
    }
}
