using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Cache;
using Core.Block;

namespace Services.Block
{
    public class CachedBlockService:ICachedBlockService
    {
        private readonly ICacheManager _cacheManager;
        private readonly IBlockService _blockService;

        private const string cachePrefix = "block_";
        private int cacheTimeMinutes = 10;

        public CachedBlockService(ICacheManager cacheManager, 
            IBlockService blockService)
        {
            _cacheManager = cacheManager;
            _blockService = blockService;
        }

        private string GetCacheKey(string id)
        {
            return cachePrefix + id;
        }

        public async Task<IBlock> GetBlockAsync(string id)
        {
            var key = GetCacheKey(id);
            if (_cacheManager.IsSet(key))
            {
                return _cacheManager.Get<IBlock>(key);
            }

            var block = await _blockService.GetBlockAsync(id);

            if (block != null)
            {
                _cacheManager.Set(key, block, cacheTimeMinutes);

                return block;
            }

            return null;
        }
    }
}
