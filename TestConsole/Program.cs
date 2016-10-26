using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            GetColoredAddressesFromBlock(indexerClient);
            //GetDeltaChanges(indexerClient, mainChain, "38DdqhVuqb36jjmxTbvBkiFPXKA8EmW9Ly");

            Console.ReadLine();
        }
        
        private static void InitContainer(IoC container, BaseSettings settings, ILog log)
        {
            container.Register<ILog>(log);

            container.BindProviders(settings, log);
            container.Register(settings);
            container.BindAzureRepositories(settings, log);
        }



        private static void GetColoredAddressesFromBlock(IndexerClient indexerClient)
        {
            var block =
    indexerClient.GetBlock(uint256.Parse("0000000000000000013d252049553e135dfb58a4b5955eaa7928405eb87f4b42"));

            var tx =
                block.Transactions.FirstOrDefault(
                    p =>
                        p.GetHash() == uint256.Parse("8ce980497bfe711b216f9643f28e9393876232e046338fff3fb39c1ca86fa5da"));



            
            Console.WriteLine("Outputs");


            foreach (var bitcoinAddress in block.GetAddresses(Network.Main))
            {
                Console.WriteLine(bitcoinAddress);
            }

            Console.WriteLine("Total {0}", block.GetAddresses(Network.Main).Count());

        }

        private static void GetDeltaChanges(IndexerClient indexerClient, ConcurrentChain mainChain, string addr)
        {
            var balanceId = BalanceIdHelper.Parse(addr, Network.Main);
            Console.WriteLine("Getting ord balances {0}", DateTime.Now.ToString("T"));
            var ordBalances = indexerClient.GetOrderedBalance(balanceId).ToArray();
            Console.WriteLine("Getting ord balances done{0}", DateTime.Now.ToString("T"));
            var balanceSheet = ordBalances.AsBalanceSheet(mainChain);
            var alTx = new List<ColoredChange>();

            var confirmed = BalanceSummaryDetailsHelper.CreateFrom(balanceSheet.Confirmed, Network.Main, true);

            foreach (var bl in balanceSheet.Confirmed)
            {
                var deltaChanges = bl.GetColoredChanges(Network.Main);

                if (deltaChanges.Any())
                {
                    //Console.WriteLine("TxId: {0} ", bl.TransactionId);
                    //Console.WriteLine();

                    foreach (var coloredChange in deltaChanges)
                    {
                        //Console.WriteLine("AssetId {0}, Quantity {1}", coloredChange.AssetId, coloredChange.Quantity);
                        alTx.Add(coloredChange);
                    }

                    //Console.WriteLine("-----------");
                }
            }


            Console.WriteLine("Summary");
            foreach (var assetGrouping in alTx.GroupBy(p => p.AssetId))
            {
                Console.WriteLine("Asset {0} Calculated {1} Actual {2}",
                    assetGrouping.Key,
                    assetGrouping.Sum(p => p.Quantity),
                    confirmed.Assets.FirstOrDefault(p => p.Asset.ToString() == assetGrouping.Key)?.Quantity);
            }
        }

    }
}
