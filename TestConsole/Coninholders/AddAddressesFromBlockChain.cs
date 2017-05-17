using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureRepositories.AssetCoinHolders;
using Common.IocContainer;
using Core.AssetBlockChanges;
using Core.AssetBlockChanges.Mongo;
using Core.Settings;
using MongoDB.Driver;
using NBitcoin;
using Providers;
using Providers.Helpers;
using Services.BalanceChanges;
using Services.MainChain;

namespace TestConsole
{
    public class AddAddressesFromBlockChain
    {
        public static async Task Run(IoC ioc)
        {
            var baseSettings = ioc.GetObject<BaseSettings>();
            var balanceChangesRepo = ioc.GetObject<IAssetBalanceChangesRepository>();
            var balanceChangesService = ioc.GetObject<BalanceChangesService>();

            var collection =
                new MongoClient(baseSettings.Db.AssetBalanceChanges.ConnectionString)
                .GetDatabase(baseSettings.Db.AssetBalanceChanges.DbName)
                .GetCollection<AddressAssetBalanceChangeMongoEntity>("asset-balances");
            var indexerClientFactory = ioc.GetObject<IndexerClientFactory>();
            var mainChainRepository = ioc.GetObject<MainChainService>();

            var okFile = "./AddAddressesFromBlockChain-ok.txt";
            var failFile = "./AddAddressesFromBlockChain-fail.txt";
            var startBlock = 300000;
            var toBlock = 1123697;
            var st = new Stopwatch();
            st.Start();
            Console.WriteLine(startBlock);
            var mainChain = await mainChainRepository.GetMainChainAsync();

            var tasks = new List<Task>();
            foreach (var i in Enumerable.Range(startBlock, toBlock - startBlock).Reverse())
            {
                Console.WriteLine(i);
                var tsk = indexerClientFactory.GetIndexerClient().GetBlock(mainChain.GetBlock(i).HashBlock).GetAddressesWithColoredMarkerAsync(baseSettings.UsedNetwork(), indexerClientFactory.GetIndexerClient())
                    .ContinueWith(async x =>
                    {
                        try
                        {
                            await balanceChangesService.SaveAddressChangesAsync(i, i, x.Result.ToArray());
                            Console.WriteLine("{0} done", i);

                            File.AppendAllLines(okFile, new[] { i.ToString(), "------" });
                        }
                        catch (Exception e)
                        {
                            File.AppendAllLines(failFile, new[] { i.ToString(), e.ToString(), "------" });
                        }

                    });
                tasks.Add(tsk);
            }





            await Task.WhenAll(tasks);

            Console.WriteLine("All done");
            Console.ReadLine();
        }
    }
}



public class Address
{
    
}