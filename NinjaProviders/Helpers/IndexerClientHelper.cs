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
}
