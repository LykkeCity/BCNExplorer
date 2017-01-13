using System;
using System.IO;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using AzureStorage;
using Common;
using Common.Files;
using Common.Log;
using Core.Settings;
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

        private readonly bool _cacheMainChainAsLocalFile;
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private ObjectCache Cache => MemoryCache.Default;

        private string FilePath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "bin", "chain.dat").ToString();
        private const string BlobContainerName = "mainchain";
        private const string BlobKeyName = "data";
        private const string CacheKey = "MainChainSource";
        

        public MainChainService(IndexerClientFactory indexerClient, IBlobStorage storage, ILog log, BaseSettings baseSettings)
        {
            _indexerClient = indexerClient;
            _log = log;
            _blobStorage = storage;

            _cacheMainChainAsLocalFile = baseSettings.CacheMainChainLocalFile;
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

                if (_cacheMainChainAsLocalFile)
                {

                    result = new ConcurrentChain(File.ReadAllBytes(FilePath));
                }
                else
                {
                    result = new ConcurrentChain((await _blobStorage.GetAsync(BlobContainerName, BlobKeyName)).ReadFully());

                    await
                        _log.WriteInfo("MainChainRepository", "GetFomPersistentCacheAsync", null,
                            "Get from blob storage done");
                }

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

                if (_cacheMainChainAsLocalFile)
                {
                    var data = memorySteam.ToArray();
                    File.WriteAllBytes(FilePath, data);
                }
                else
                {
                    await _blobStorage.SaveBlobAsync(BlobContainerName, BlobKeyName, memorySteam);
                    await _log.WriteInfo("MainChainRepository", "SetToPersistentCacheAsync", null, "Save to blob storage done");
                }
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
