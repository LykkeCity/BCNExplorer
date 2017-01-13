using System;
using System.IO;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using AzureStorage;
using Common;
using Common.Files;
using Common.Log;
using NBitcoin;
using NBitcoin.Indexer;
using Providers;

namespace Services.MainChain
{
    public class MainChainService
    {
        private readonly IndexerClientFactory _indexerClient;
        private readonly ILog _log;
        private readonly IBlobStorage _blobStorage;

        private static SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private ObjectCache Cache => MemoryCache.Default;

        private string FilePath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "bin", "chain.dat").ToString();
        private const string BlobContainerName = "mainchain";
        private const string BlobKeyName = "data";
        private const string CacheKey = "MainChainSource";

        public MainChainService(IndexerClientFactory indexerClient, IBlobStorage storage, ILog log)
        {
            _indexerClient = indexerClient;
            _log = log;
            _blobStorage = storage;
        }

        private ConcurrentChain GetFromTemporaryCache()
        {
            return Cache[CacheKey] as ConcurrentChain;
        }

        private async Task<ConcurrentChain> GetFromPersistentCacheAsync()
        {
            try
            {
                ConcurrentChain result;
#if (DEBUG)
                result = new  ConcurrentChain(File.ReadAllBytes(FilePath));
#endif
#if (!DEBUG)
                result = new ConcurrentChain((await _blobStorage.GetAsync(BlobContainerName, BlobKeyName)).ReadFully());

                await
                    _log.WriteInfo("MainChainRepository", "GetFomPersistentCacheAsync", null,
                        "Get from blob storage done");
#endif

                return result;

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
#if (DEBUG)                
                var data = memorySteam.ToArray();
                File.WriteAllBytes(FilePath, data);
#endif
#if (!DEBUG)
                await _blobStorage.SaveBlobAsync(BlobContainerName, BlobKeyName, memorySteam);
                await _log.WriteInfo("MainChainRepository", "SetToPersistentCacheAsync", null, "Save to blob storage done");
#endif
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
                await SetToPersistentCacheAsync(result);
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
