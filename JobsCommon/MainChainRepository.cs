using System;
using System.Threading.Tasks;
using Common.Files;
using Common.Log;
using NBitcoin;
using NBitcoin.Indexer;

namespace JobsCommon
{
    public class MainChainRepository
    {
        private readonly IndexerClient _indexerClient;
        private readonly ILog _log;

        private const string FilePath = "./chain.dat";

        public MainChainRepository(IndexerClient indexerClient, ILog log)
        {
            _indexerClient = indexerClient;
            _log = log;
        }

        private async Task<ConcurrentChain> GetFromCacheAsync()
        {
            try
            {
                return new ConcurrentChain(await ReadWriteHelper.ReadAllFileAsync(FilePath));
            }
            catch (Exception e)
            {
                await _log.WriteError("MainChainRepository", "GetFromCacheAsync", null, e);

                return null;
            }
        }

        private Task<ConcurrentChain> GetFromIndexerAsync()
        {
            return Task.Run(() => _indexerClient.GetMainChain());
        }

        private async Task CacheChainAsync(ConcurrentChain chain)
        {
            try
            {
                await ReadWriteHelper.WriteAsync(FilePath, chain.ToBytes());
            }
            catch (Exception e)
            {
                await _log.WriteError("MainChainRepository", "CacheChainAsync", null, e);
            }
        }

        public async Task<ConcurrentChain> GetMainChainAsync()
        {
            var result = await GetFromCacheAsync() ?? await GetFromIndexerAsync();
            var iniTip = result.Tip.Height;

            var chainChanges = _indexerClient.GetChainChangesUntilFork(result.Tip, false);
            chainChanges.UpdateChain(result);

            if (iniTip != result.Height)
            {
                await CacheChainAsync(result);
            }

            return result;
        } 
    }
}
