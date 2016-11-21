using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureRepositories;
using AzureRepositories.AssetCoinHolders;
using Common.IocContainer;
using Common.Log;
using Core.AssetBlockChanges.Mongo;
using Core.Settings;
using JobsCommon;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using NBitcoin;
using Providers;
using Providers.Helpers;
using SQLRepositories.Context;
using SQLRepositories.DbModels;

namespace TestConsole
{
    public class MongoFillBalance
    {
        public static async Task Run(IoC ioc)
        {
            var balanceChangesRepo = ioc.GetObject<IAssetBalanceChangesRepository>();
            var indexerClientFactory = ioc.GetObject<IndexerClientFactory>();
            var contextFactory = ioc.GetObject<BcnExplolerFactory>();
            var baseSettings = ioc.GetObject<BaseSettings>();
            var log = ioc.GetObject<ILog>();

            var mainChainRepository = ioc.GetObject<MainChainRepository>();

            IEnumerable<AddressEntity> addr;
            var collection = 
                new MongoClient(baseSettings.Db.AssetBalanceChanges.ConnectionString)
                .GetDatabase(baseSettings.Db.AssetBalanceChanges.DbName)
                .GetCollection<AddressAssetBalanceChangeMongoEntity>("asset-balances");
            var parsedAddresses =
                (await collection.Find(p => true).Project(p => p.ColoredAddress).Limit(int.MaxValue).ToListAsync()).Distinct();
            using (var db = contextFactory.GetContext())
            {
                addr = db.Addresses.ToList().Where(p=>!parsedAddresses.Contains(p.ColoredAddress));
                //addr =
                //    db.BalanceChanges.Where(p => p.AssetId == "AWm6LaxuJgUQqJ372qeiUxXhxRWTXfpzog")
                //        .Select(p => p.AddressEntity)
                //        .Distinct()
                //        .ToList();
                //addr = db.Addresses.Where(p => p.ColoredAddress== "akYc7BCwLpf1JWTnQdj8WN94Gajokn8MEhT").ToList();
            }
            var file = "./errors.txt";

            var semaphore = new SemaphoreSlim(100);
            var tasksToAwait = new List<Task>();
            var counter = addr.Count();
            var st = new Stopwatch();
            st.Start();
            var mainChain = await mainChainRepository.GetMainChainAsync();
            var to = mainChain.Height;
            to = 439808;
            File.AppendAllLines(file, new[] { to.ToString(),  "----------------" });
            Console.WriteLine(addr.Count());
            foreach (var address in addr.Select(p => p.ColoredAddress).Distinct().ToList().OrderBy(p=>p))
            {
                var balanceId = BalanceIdHelper.Parse(address, Network.Main);
                Console.WriteLine(address);
                var changesTask = indexerClientFactory.GetIndexerClient()
                    .GetConfirmedBalanceChangesAsync(balanceId, mainChain, semaphore, 0, to)
                    .ContinueWith(
                        async task =>
                        {
                            try
                            {
                                counter--;
                                var counterTemp = counter;
                                Console.WriteLine("-Continue started {0} {1}", st.Elapsed.ToString("g"), counter);

                                var coloredChanges = task.Result.SelectMany(p => p.GetColoredChanges(Network.Main)).ToList();
                                if (coloredChanges.Any())
                                {
                                    var balanceChanges = coloredChanges.Select(p => AssetBalanceChanges.Create(p.AssetId,
                                       p.Quantity,
                                       p.BlockHash,
                                       mainChain.GetBlock(uint256.Parse(p.BlockHash)).Height,
                                       p.TransactionHash));

                                    await balanceChangesRepo.AddAsync(address, balanceChanges);
                                }

                                Console.WriteLine("--Continue Done {0} {1} {2}", counterTemp, st.Elapsed.ToString("g"), DateTime.Now.ToString("t"));
                            }
                            catch (Exception e)
                            {

                                Console.WriteLine(e.ToString());
                                File.AppendAllLines(file, new[] { address, e.ToString(), "----------------" });
                            }

                        });

                tasksToAwait.Add(changesTask.Unwrap());
            }

            await Task.WhenAll(tasksToAwait);
            Console.WriteLine("All done");
            Console.ReadLine();
        }
    }
}
