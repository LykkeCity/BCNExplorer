using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Core.AssetBlockChanges;
using SQLRepositories.Context;
using SQLRepositories.DbModels;

namespace SQLRepositories.Repositories
{
    public class TransactionRepository:ITransactionRepository
    {
        private readonly BcnExplolerFactory _bcnExplolerFactory;

        public TransactionRepository(BcnExplolerFactory bcnExplolerFactory)
        {
            _bcnExplolerFactory = bcnExplolerFactory;
        }

        public async Task AddAsync(ITransaction[] transactions)
        {
            using (var db = _bcnExplolerFactory.GetContext())
            {
                var existed = await db.Transactions.ToListAsync();
                var posted = transactions.Where(p => p != null).Select(TransactionEntity.Create).Distinct(TransactionEntity.TransactionHashComparer);

                //Do not add existed in db 
                var entitiesToAdd =
                    posted.Where(p => !existed.Contains(p, TransactionEntity.TransactionHashComparer));
                db.Transactions.AddRange(entitiesToAdd);

                await db.SaveChangesAsync();
            }
        }
    }
}
