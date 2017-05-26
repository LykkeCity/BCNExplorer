using System;
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
    }
}
