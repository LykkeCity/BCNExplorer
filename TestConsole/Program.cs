using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AzureRepositories;
using AzureRepositories.Binders;
using Common.Files;
using Common.IocContainer;
using Common.Log;
using Core.Settings;
using JobsCommon;
using Microsoft.WindowsAzure.Storage.Auth;
using NBitcoin;
using NBitcoin.Indexer;
using NBitcoin.OpenAsset;
using Providers;
using Providers.Helpers;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = GeneralSettingsReader.ReadGeneralSettings<BaseSettings>(JobsConnectionStringSettings.ConnectionString);
     
            var container = new IoC();
            InitContainer(container, settings, new LogToConsole());

            var indexerClient = container.GetObject<IndexerClient>();
            var fileName = "./chain.dat";
            //var mainChain = indexerClient.GetMainChain();
            //File.WriteAllBytes(fileName,mainChain.ToBytes());

            var mainChain = new ConcurrentChain(ReadWriteHelper.ReadAllFileAsync(fileName).Result);
            Console.WriteLine("Getting chain changes");
            var chainChanges = indexerClient.GetChainChangesUntilFork(mainChain.Tip, false).ToArray();
            Console.WriteLine("Getting chain done");
            chainChanges.UpdateChain(mainChain);

            File.WriteAllBytes(fileName,mainChain.ToBytes());

            var st = new Stopwatch();
            st.Start();

            var coloredAddresses = GetColoredAddressesFromBlock(indexerClient,
                "0000000000000000029559b0665cacb4470eda0696a69744263e82e7e4d0f27d").ToArray();

            var checkTasks = new List<Task>();
            var st1 = new Stopwatch();
            st1.Start();

            var counter = coloredAddresses.Length;
            var semaphore = new SemaphoreSlim(100);
            foreach (var address in coloredAddresses)
            {
                var balanceId = BalanceIdHelper.Parse(address.ToString(), Network.Main);
                //Console.WriteLine("Started {0}", address.ToString());

                checkTasks.Add(indexerClient.GetConfirmedBalanceChangesAsync(balanceId, mainChain, semaphore).ContinueWith(
                    p =>
                    {
                        counter--;
                        Console.WriteLine("elapsed {0}, counter {1}", st.Elapsed.ToString("g"), counter);
                    }));
            }
            //GetDeltaChanges(indexerClient, mainChain, "16LjqTr9qZP2vRkHFR731Gui1uEnBWxgQZ");

            st.Stop();

            Console.WriteLine("_____________________");
            Console.WriteLine("Done {0}", st.Elapsed.ToString("g"));
            

            Console.ReadLine();
        }
        
        private static void InitContainer(IoC container, BaseSettings settings, ILog log)
        {
            container.Register<ILog>(log);

            container.BindProviders(settings, log);
            container.Register(settings);
            container.BindAzureRepositories(settings, log);
        }



        private static IEnumerable<BitcoinAddress> GetColoredAddressesFromBlock(IndexerClient indexerClient, string blockId)
        {
            var block =
    indexerClient.GetBlock(uint256.Parse(blockId));
            

            
            Console.WriteLine("Outputs");


            foreach (var bitcoinAddress in block.GetAddresses(Network.Main))
            {
                yield return bitcoinAddress;
                Console.WriteLine(bitcoinAddress);
            }

            Console.WriteLine("Total {0}", block.GetAddresses(Network.Main).Count());

        }

    }
}
