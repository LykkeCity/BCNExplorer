using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Cache;
using Core.Transaction;

namespace Services.Transaction
{
    public class CachedTransactionService: ICachedTransactionService
    {
        private readonly ICacheManager _cacheManager;
        private readonly ITransactionService _transactionService;

        private const string CachePrefix = "transaction_";
        private int cacheTimeMinutes = 1;

        private string GetCacheKey(string id)
        {
            return CachePrefix + id;
        }

        public CachedTransactionService(ICacheManager cacheManager, ITransactionService transactionService)
        {
            _cacheManager = cacheManager;
            _transactionService = transactionService;
        }

        public async Task<ITransaction> GetAsync(string id)
        {
            var key = GetCacheKey(id);
            if (_cacheManager.IsSet(key))
            {
                return _cacheManager.Get<ITransaction>(key);
            }

            var transaction = await _transactionService.GetAsync(id);

            if (transaction != null)
            {
                _cacheManager.Set(key, transaction, cacheTimeMinutes);

                return transaction;
            }

            return null;
        }

        public async Task<IEnumerable<ITransaction>> GetAsync(IEnumerable<string> ids)
        {
            var result = new ConcurrentStack<ITransaction>();

            var idList = ids as IList<string> ?? ids.ToList();

            var loadTransactionTasks = idList.Select(id => _transactionService.GetAsync(id).ContinueWith(task =>
            {
                if (task.Result != null)
                {
                    result.Push(task.Result);
                }
            }));

            await Task.WhenAll(loadTransactionTasks);
            
            return result.OrderBy(p => idList.IndexOf(p.TransactionId));
        }
    }
}
