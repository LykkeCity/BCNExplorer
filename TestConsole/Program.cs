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

            var chainChanges = indexerClient.GetChainChangesUntilFork(mainChain.Tip, false).ToArray();

            chainChanges.UpdateChain(mainChain);

            File.WriteAllBytes(fileName,mainChain.ToBytes());   
                 
            var balanceId = BalanceIdHelper.Parse("akCk9f5nYnwjaKUvSURWmG9FjxQYaTKUU4T", Network.Main);
            var ordBalances = indexerClient.GetOrderedBalance(balanceId).ToArray();
            var balanceSheet = ordBalances.AsBalanceSheet(mainChain);
            var alTx = new List<ColoredChange>();

            var confirmed = BalanceSummaryDetailsHelper.CreateFrom(balanceSheet.Confirmed, Network.Main, true);
            
            foreach (var bl in balanceSheet.Confirmed)
            {
                var deltaChanges = bl.GetColoredChanges(Network.Main);

                if (deltaChanges.Any())
                {
                    Console.WriteLine("TxId: {0} ", bl.TransactionId);
                    Console.WriteLine();

                    foreach (var coloredChange in deltaChanges)
                    {
                        Console.WriteLine("AssetId {0}, Quantity {1}", coloredChange.AssetId, coloredChange.Quantity);
                        alTx.Add(coloredChange);
                    }

                    Console.WriteLine("-----------");
                }
            }


            Console.WriteLine("Summary");
            foreach (var assetGrouping in alTx.GroupBy(p => p.AssetId))
            {
                Console.WriteLine("Asset {0} Calculated {1} Actual {2}", 
                    assetGrouping.Key, 
                    assetGrouping.Sum(p => p.Quantity),
                    confirmed.Assets.FirstOrDefault(p=>p.Asset.ToString()== assetGrouping.Key)?.Quantity);
            }


            Console.ReadLine();
        }
        
        private static void InitContainer(IoC container, BaseSettings settings, ILog log)
        {
            container.Register<ILog>(log);

            container.BindProviders(settings, log);
            container.Register(settings);
            container.BindAzureRepositories(settings, log);
        }


        

        
    }
}
