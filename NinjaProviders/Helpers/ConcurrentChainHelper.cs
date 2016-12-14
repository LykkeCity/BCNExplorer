using System;
using System.Linq;
using NBitcoin;

namespace Providers.Helpers
{
    public static class ConcurrentChainHelper
    {
        public static ChainedBlock GetClosestToTimeBlock(this ConcurrentChain chain, DateTime time)
        {
            return chain.ToEnumerable(true).FirstOrDefault(p=>p.Header.BlockTime<=time);
        }
    }
}
