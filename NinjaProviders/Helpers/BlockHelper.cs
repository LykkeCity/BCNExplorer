using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;
using NBitcoin.Indexer;
using NBitcoin.OpenAsset;

namespace Providers.Helpers
{
    public static class BlockHelper
    {
        public static string GetBlockId(this BlockHeader header)
        {
            return header?.GetHash().AsBitcoinSerializable().Value.ToString();
        }

        public static async Task<IEnumerable<BitcoinAddress>> GetAddressesWithColoredMarkerAsync(this Block block, Network network, IndexerClient indexerClient)
        {
            var result = new List<BitcoinAddress>();
            var coloredTxs = block?.Transactions?.Where(p => p.HasValidColoredMarker());
            foreach (var transaction in coloredTxs)
            {
                result.AddRange(transaction.GetOutputAddresses(network));
            }
            var loadedTxs = await indexerClient.GetTransactionsAsync(true, coloredTxs.Select(p => p.GetHash()).ToArray());
            result.AddRange(loadedTxs.SelectMany(p=>p.GetInputAddresses(network)));
            return result;
        }

        public static IEnumerable<BitcoinAddress> GetOutputAddresses(this Transaction transaction, Network network)
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

        private static IEnumerable<BitcoinAddress> GetInputAddresses(this TransactionEntry tx, Network network)
        {
            for (int i = 0; i < tx.Transaction.Inputs.Count; i++)
            {
                var address = tx.SpentCoins[i].TxOut.ScriptPubKey.GetDestinationAddress(network);
                if (address != null)
                {
                    yield return address;
                }
            }
        }
    }
}
