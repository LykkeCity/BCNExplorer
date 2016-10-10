using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Auth;
using NBitcoin;
using NBitcoin.Indexer;
using NBitcoin.OpenAsset;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {

            //var _client = new IndexerClient(configurationMainNet);

            //var block =
            //    _client.GetBlock(
            //        uint256.Parse("0000000000000000011efc2e2f7f771370597a835f63dd945df2f5467223303b"));

            //var txId = block.GetHash().AsBitcoinSerializable().Value.ToString();
            //var tx = block.Transactions.FirstOrDefault(p=>p.GetHash()== uint256.Parse("0c0a21b640b1011ed397138d76b81375d35c10e419b52fc887677634dd7bcd4f"));

            //var t2 = tx.GetColoredMarker().GetMetadataUrl();
            //var url = t2;

        }
    }
}
