using System;
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
     BalanceId balanceId, ConcurrentChain mainChain, int fromBlockHeight, int toBlock)
        {

            //await semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                var startBlock = mainChain.GetBlock(fromBlockHeight);
                var stopBlock = mainChain.GetBlock(toBlock);
                var balanceQuery = new BalanceQuery();
                balanceQuery.RawOrdering = true;
                balanceQuery.From = new ConfirmedBalanceLocator(startBlock.Height, startBlock.HashBlock);
                balanceQuery.To = new ConfirmedBalanceLocator(stopBlock.Height, stopBlock.HashBlock);
                
                //return indexerClient.GetOrderedBalance(balanceId, balanceQuery)
                //    .TakeWhile(_ => _.BlockId == null || _.Height >= stopBlock.Height || _.Height <= stopBlock.Height)
                //    .AsBalanceSheet(mainChain).Confirmed;

                var ordBalances = await Task.WhenAll(indexerClient.GetOrderedBalanceAsync(balanceId, balanceQuery)).ConfigureAwait(false);
                return ordBalances
                    .SelectMany(p => p.ToArray())
                    .AsBalanceSheet(mainChain).Confirmed;
            }
            finally
            {
                //semaphore.Release();
            }

        }
    }
}
