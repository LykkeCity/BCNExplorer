using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.IocContainer;
using NBitcoin;
using NBitcoin.Indexer;
using Providers;
using Providers.Helpers;

namespace TestConsole
{
    public static class ParseBlock
    {
        public static async Task Run(IoC container)
        {
            Console.WriteLine("Parse Block started ");
            var client = container.GetObject<IndexerClientFactory>().GetIndexerClient();
            var bl = client.GetBlock(uint256.Parse("00000000000000000174225043caa4ba30d4527ef479380f3aa249f624c1e617"));
            var tx =
                bl.Transactions.First(
                    p => p.GetTransactionId() == "0916e9658e506afc2c99a1b44b2b018255c1dbf4195729bda7d29eaebaed6653");
            var st = new Stopwatch();
            st.Start();
            var txIds = bl.Transactions.Select(p => p.GetHash()).ToArray();
            var txs = new ConcurrentBag<TransactionEntry>();

            var tasks = new List<Task>();
            foreach (var batch in txIds.Batch(50))
            {
                var task = client.GetTransactionsAsync(false, false, batch.ToArray()).ContinueWith(p =>
                {
                    foreach (var transactionEntry in p.Result)
                    {
                        txs.Add(transactionEntry);
                    }
                    Console.WriteLine("Batch done");
                });
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            st.Stop();
            Console.WriteLine("Elapsed " + st.Elapsed.ToString("g"));
            //Console.WriteLine("Inputs");

            //var tx1 = txs.First();
            //foreach (var txout in tx1.SpentCoins.Select(p=>p.TxOut))
            //{
            //    Console.WriteLine(txout.ScriptPubKey.GetDestinationAddress(Network.Main).ToWif());
            //}

            Console.WriteLine("Parse Block done ");
            Console.ReadLine();
        }
    }
}
