using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.AssetBlockChanges;
using SQLRepositories.Context;
using SQLRepositories.DbModels;

namespace SQLRepositories.Repositories
{
    public class TransactionRepository:ITransactionRepository
    {
        private readonly BcnExplolerFactory _bcnExplolerFactory;

        private static readonly SemaphoreSlim _lock = new SemaphoreSlim(initialCount: 1);

        public TransactionRepository(BcnExplolerFactory bcnExplolerFactory)
        {
            _bcnExplolerFactory = bcnExplolerFactory;
        }

        public async Task AddAsync(ITransaction[] transactions)
        {
            try
            {
                await _lock.WaitAsync();
                using (var db = _bcnExplolerFactory.GetContext())
                {
                    var posted = transactions
                        .Where(p => p != null)
                        .Select(TransactionEntity.Create)
                        .Distinct(TransactionEntity.TransactionHashComparer)
                        .ToList();

                    var postedHashes = posted.Select(p => p.Hash).ToList();
                    var existed = db.Transactions.Where(p => postedHashes.Contains(p.Hash)).ToList();
                    //Do not add existed in db 
                    var entitiesToAdd =
                        posted.Where(p => !existed.Contains(p, TransactionEntity.TransactionHashComparer))
                        .ToList();
                    db.Transactions.AddRange(entitiesToAdd);
                    await db.SaveChangesAsync();
                }
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
