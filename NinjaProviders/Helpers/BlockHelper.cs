using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;

namespace Providers.Helpers
{
    public static class BlockHelper
    {
        public static string GetBlockId(this BlockHeader header)
        {
            return header?.GetHash().AsBitcoinSerializable().Value.ToString();
        }
    }
}
