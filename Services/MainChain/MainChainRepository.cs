﻿using System;
using System.IO;
using System.Runtime.Caching;
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

        private const string FilePath = "./chain.dat";

        private ObjectCache Cache => MemoryCache.Default;
        private const string CacheKey = "MainChain";

        public MainChainRepository(IndexerClientFactory indexerClient, ILog log)
        {
            _indexerClient = indexerClient;
            _log = log;
        }

        private async Task<ConcurrentChain> GetFromCacheAsync()
        {
            try
            {
                var memoryCached = Cache[CacheKey] as ConcurrentChain;

                if (memoryCached != null)
                {
                    return memoryCached;
                }

                var result =  new ConcurrentChain(await ReadWriteHelper.ReadAllFileAsync(FilePath));

                Cache.Set(CacheKey, result, ObjectCache.InfiniteAbsoluteExpiration);

                return result;
            }
            catch (Exception e)
            {
                await _log.WriteError("MainChainRepository", "GetFromCacheAsync", null, e);

                return null;
            }
        }

        private async Task<ConcurrentChain> GetFromIndexerAsync()
        {
            return _indexerClient.GetIndexerClient().GetMainChain();
        }

        private async Task CacheChainAsync(ConcurrentChain chain)
        {
            try
            {
                Cache.Set(CacheKey, chain, ObjectCache.InfiniteAbsoluteExpiration);
                File.WriteAllBytes(FilePath, chain.ToBytes());

                //await ReadWriteHelper.WriteAsync(FilePath, chain.ToBytes());
            }
            catch (Exception e)
            {
                await _log.WriteError("MainChainRepository", "CacheChainAsync", null, e);
            }
        }

        public async Task<ConcurrentChain> GetMainChainAsync()
        {
            var iniTip = 0;
            var result = await GetFromCacheAsync();

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
                await CacheChainAsync(result);
            }

            return result;
        } 
    }
}