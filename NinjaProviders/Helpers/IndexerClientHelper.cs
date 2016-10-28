using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NBitcoin;
using NBitcoin.Indexer;

namespace Providers.Helpers
{
    public static class IndexerClientHelper
    {
        public static Block GetBlock(this IndexerClient indexerClient, string blockId)
        {
            return indexerClient.GetBlock(uint256.Parse(blockId));
        }
    }

    public static class ConfirmedBalanceChangesHelper
    {
        public static async Task<IEnumerable<OrderedBalanceChange>> GetConfirmedBalanceChangesAsync(this IndexerClient indexerClient,
     BalanceId balanceId, ConcurrentChain mainChain, SemaphoreSlim semaphore)
        {

            await semaphore.WaitAsync();
            try
            {
                var startBlock = mainChain.GetBlock(mainChain.Tip.Height - 1);
                var balanceQuery = new BalanceQuery();
                balanceQuery.RawOrdering = true;
                balanceQuery.From = new ConfirmedBalanceLocator(startBlock.Height, startBlock.HashBlock);
                balanceQuery.To = new ConfirmedBalanceLocator(mainChain.Tip.Height, mainChain.Tip.HashBlock);
                var ordBalances = await Task.WhenAll(indexerClient.GetOrderedBalanceAsync(balanceId, balanceQuery));

                return ordBalances.SelectMany(p => p.ToArray()).AsBalanceSheet(mainChain).Confirmed;
            }
            finally
            {
                semaphore.Release();
            }

        }
    }
}
