using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Files;
using Common.IocContainer;
using NBitcoin;
using NBitcoin.Indexer;
using Providers.Helpers;

namespace TestConsole
{
    public class TestGettingChainChanges
    {
        public static void Run(IoC container)
        {
            var indexerClient = container.GetObject<IndexerClient>();
            var fileName = "./chain.dat";
            //var mainChain = indexerClient.GetMainChain();
            //File.WriteAllBytes(fileName, mainChain.ToBytes());

            var mainChain = new ConcurrentChain(ReadWriteHelper.ReadAllFileAsync(fileName).Result);
            Console.WriteLine("Getting chain changes");
            var chainChanges = indexerClient.GetChainChangesUntilFork(mainChain.Tip, false).ToArray();
            Console.WriteLine("Getting chain done");
            chainChanges.UpdateChain(mainChain);

            File.WriteAllBytes(fileName, mainChain.ToBytes());

            var st = new Stopwatch();
            st.Start();

            var block =
                mainChain.GetBlock(uint256.Parse("0000000000000000029559b0665cacb4470eda0696a69744263e82e7e4d0f27d"));
            var coloredAddresses = GetColoredAddressesFromBlock(indexerClient, block.HashBlock.ToString()).ToArray();

            var checkTasks = new List<Task>();
            var st1 = new Stopwatch();
            st1.Start();

            var counter = coloredAddresses.Length;
            var semaphore = new SemaphoreSlim(100);
            foreach (var address in coloredAddresses)
            {
                var balanceId = BalanceIdHelper.Parse(address.ToString(), Network.Main);
                //Console.WriteLine("Started {0}", address.ToString());

                checkTasks.Add(indexerClient.GetConfirmedBalanceChangesAsync(balanceId, mainChain, semaphore, block.Height-1, block.Height).ContinueWith(
                    p =>
                    {
                        counter--;
                        Console.WriteLine("Changes {0}", string.Join(", ", p.Result.Select(x=>x.Amount)));
                        Console.WriteLine("elapsed {0}, counter {1}", st.Elapsed.ToString("g"), counter);
                    }));
            }
            //GetDeltaChanges(indexerClient, mainChain, "16LjqTr9qZP2vRkHFR731Gui1uEnBWxgQZ");

            st.Stop();

            Console.WriteLine("_____________________");
            Console.WriteLine("Done {0}", st.Elapsed.ToString("g"));


            Console.ReadLine();
        }

        private static IEnumerable<BitcoinAddress> GetColoredAddressesFromBlock(IndexerClient indexerClient, string blockId)
        {
            var block = indexerClient.GetBlock(uint256.Parse(blockId));

            throw new NotImplementedException();
            //foreach (var bitcoinAddress in block.GetAddressesWithColoredMarker(Network.Main))
            //{
            //    yield return bitcoinAddress;
            //    Console.WriteLine(bitcoinAddress);
            //}
        }
    }
}
