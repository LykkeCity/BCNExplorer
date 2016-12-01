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
using Services.MainChain;

namespace TestConsole
{
    public class AddAddressesFromBlockChain
    {
        public static async Task Run(IoC ioc)
        {
            var addressRepo = ioc.GetObject<IAddressRepository>();
            var baseSettings = ioc.GetObject<BaseSettings>();
            var balanceChangesRepo = ioc.GetObject<IAssetBalanceChangesRepository>();

            var collection =
                new MongoClient(baseSettings.Db.AssetBalanceChanges.ConnectionString)
                .GetDatabase(baseSettings.Db.AssetBalanceChanges.DbName)
                .GetCollection<AddressAssetBalanceChangeMongoEntity>("asset-balances");
            var indexerClientFactory = ioc.GetObject<IndexerClientFactory>();
            var mainChainRepository = ioc.GetObject<MainChainRepository>();

            var okFile = "./AddAddressesFromBlockChain-ok.txt";
            var failFile = "./AddAddressesFromBlockChain-fail.txt";
            var startBlock = 416434;
            var toBlock = 439808;
            var st = new Stopwatch();
            st.Start();
            Console.WriteLine(startBlock);
            var mainChain = await mainChainRepository.GetMainChainAsync();
            
            var tasks = new List<Task>();
            foreach (var i in Enumerable.Range(startBlock, toBlock- startBlock))
            {
                Console.WriteLine(i);
                var tsk= indexerClientFactory.GetIndexerClient().GetBlock(mainChain.GetBlock(i).HashBlock).GetAddressesWithColoredMarkerAsync(baseSettings.UsedNetwork(), indexerClientFactory.GetIndexerClient())
                    .ContinueWith(async x =>
                    {
                        try
                        {
                            Console.WriteLine(st.Elapsed.ToString("g"));
                            var addr = x.Result.Select(p => new BitcoinColoredAddress(p).ToString()).Select(p=> new Address {ColoredAddress = p}).ToArray();
                            //File.AppendAllLines(okFile, new[] { i.ToString(), string.Join(",", addr.Select(p => p.ColoredAddress)), "--------" });

                            await addressRepo.AddAsync(addr);
                            Console.WriteLine("{0} done", i);
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
