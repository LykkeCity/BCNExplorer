using System;
using System.IO;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using Common.Files;
using Common.Log;
using NBitcoin;
using NBitcoin.Indexer;
using Providers;

namespace Services.MainChain
{
    public class MainChainRepository
    {
        private readonly IndexerClientFactory _indexerClient;
        private readonly ILog _log;
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        private string FilePath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "bin", "chain.dat").ToString();

        private ObjectCache Cache => MemoryCache.Default;
        private const string CacheKey = "MainChainSource";

        public MainChainRepository(IndexerClientFactory indexerClient, ILog log)
        {
            _indexerClient = indexerClient;
            _log = log;
        }

        //private async Task<ConcurrentChain> GetFromCacheAsync()
        //{
        //    try
        //    {
        //        var memoryCached = Cache[CacheKey] as ConcurrentChain;

        //        if (memoryCached != null)
        //        {
        //            return memoryCached;
        //        }

        //        var result =  new ConcurrentChain(await ReadWriteHelper.ReadAllFileAsync(FilePath));
                
        //        Cache.Set(CacheKey, result, ObjectCache.InfiniteAbsoluteExpiration);

        //        return result;
        //    }
        //    catch (Exception e)
        //    {
        //        await _log.WriteError("MainChainRepository", "GetFromCacheAsync", null, e);

        //        return null;
        //    }
        //}

        private ConcurrentChain GetFromTemporaryCache()
        {
            return Cache[CacheKey] as ConcurrentChain;
        }

        private async Task<ConcurrentChain> GetFromPersistentCacheAsync()
        {
            try
            {
                return new ConcurrentChain(File.ReadAllBytes(FilePath));
            }
            catch (Exception e)
            {
                await _log.WriteError("MainChainRepository", "GetFromPersistentCacheAsync", null, e);

                return null;
            }
        }

        private async Task<ConcurrentChain> GetFromIndexerAsync()
        {
            return _indexerClient.GetIndexerClient().GetMainChain();
        }

        private async Task SetToPersistentCacheAsync(ConcurrentChain chain)
        {
            try
            {
                await _semaphore.WaitAsync();
                
                var memorySteam  = new MemoryStream();
                chain.WriteTo(memorySteam);
                File.WriteAllBytes(FilePath, memorySteam.ToArray());
            }
            catch (Exception e)
            {
                await _log.WriteError("MainChainRepository", "CacheChainAsync", null, e);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private void SetToTemporaryCache(ConcurrentChain chain)
        {
            Cache.Set(CacheKey, chain, ObjectCache.InfiniteAbsoluteExpiration);
        } 

        public async Task<ConcurrentChain> GetMainChainAsync()
        {
            var iniTip = 0;
            
            var result = GetFromTemporaryCache() ?? await GetFromPersistentCacheAsync();

            if (result != null)
            {
                iniTip = result.Tip.Height;
                var chainChanges = _indexerClient.GetIndexerClient().GetChainChangesUntilFork(result.Tip, false);
                chainChanges.UpdateChain(result);
            }
            else
            {
                result = await GetFromIndexerAsync();
            }

            if (iniTip != result.Height)
            {
                await SetToPersistentCacheAsync(result);
            }

            SetToTemporaryCache(result);

            return result;
        }
    }
}
