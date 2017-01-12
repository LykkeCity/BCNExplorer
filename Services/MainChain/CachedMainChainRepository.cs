using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using Common.Cache;
using NBitcoin;

namespace Services.MainChain
{
    public class CachedMainChainRepository
    {
        private const string CacheKey = "MainChainCached";

        private readonly MainChainRepository _sourceRepository;
        private readonly ICacheManager _cacheManager;
        private readonly int _cachedTimeInMinutes;

        public CachedMainChainRepository(MainChainRepository sourceRepository, ICacheManager cacheManager, int cachedTimeInMinutes)
        {
            _sourceRepository = sourceRepository;
            _cacheManager = cacheManager;
            _cachedTimeInMinutes = cachedTimeInMinutes;
        }

        public async Task<ConcurrentChain> GetMainChainAsync()
        {
            var cached = Get();

            if (cached == null)
            {
                await ReloadAsync();
                cached = Get();
            }

            return cached;
        }

        private ConcurrentChain Get()
        {
            return _cacheManager.Get<ConcurrentChain>(CacheKey);
        }

        public async Task  ReloadAsync()
        {
            var updated = await _sourceRepository.GetMainChainAsync();

            _cacheManager.Set(CacheKey, updated, _cachedTimeInMinutes);
        }
    }
}
