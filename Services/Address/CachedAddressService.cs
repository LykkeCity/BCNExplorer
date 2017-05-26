using System;
using System.Threading.Tasks;
using Common.Cache;
using Core.AddressService;
using Core.Block;

namespace Services.Address
{
    public class CachedAddressService:ICachedAddressService
    {
        private readonly ICacheManager _cacheManager;
        private readonly IAddressService _addressService;

        private const string CachePrefix = "address";
        private int cacheTimeMinutes = 1;

        public CachedAddressService(ICacheManager cacheManager, IAddressService addressService)
        {
            _cacheManager = cacheManager;
            _addressService = addressService;
        }

        private string GetCacheKey(string id)
        {
            return CachePrefix + id;
        }

        public async Task<IAddressTransactions> GetTransactions(string id)
        {
            var key = GetCacheKey(id);
            if (_cacheManager.IsSet(key))
            {
                return _cacheManager.Get<IAddressTransactions>(key);
            }

            var result = await _addressService.GetTransactions(id);

            if (result != null)
            {
                _cacheManager.Set(key, result, cacheTimeMinutes);

                return result;
            }

            return null;
        }
    }
}
