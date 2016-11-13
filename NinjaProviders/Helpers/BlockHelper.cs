﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;
using NBitcoin.OpenAsset;

namespace Providers.Helpers
{
    public static class BlockHelper
    {
        public static string GetBlockId(this BlockHeader header)
        {
            return header?.GetHash().AsBitcoinSerializable().Value.ToString();
        }

        public static IEnumerable<BitcoinAddress> GetAddressesWithColoredMarker(this Block block, Network network)
        {
            var result = new List<BitcoinAddress>();
            var coloredTxs = block.Transactions.Where(p => p.HasValidColoredMarker());

            foreach (var tx in coloredTxs)
            {
                result.AddRange(tx.GetAddresses(network));
            }

            return result;
        }

        public static IEnumerable<BitcoinAddress> GetAddresses(this Transaction transaction, Network network)
        {
            foreach (var txOut in transaction.Outputs)
            {
                var addr = txOut.ScriptPubKey.GetDestinationAddress(network);
                if (addr != null)
                {
                    yield return addr;
                }
            }
        }
    }
}
