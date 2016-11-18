using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.IocContainer;
using Core.AssetBlockChanges.Mongo;
using JobsCommon;
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

            var mainChainRepository = ioc.GetObject<MainChainRepository>();

            IEnumerable<AddressEntity> addr;

            using (var db = contextFactory.GetContext())
            {
                //addr = db.Addresses.Take(1).ToList();
                addr =
                    db.BalanceChanges.Where(p => p.AssetId == "ARPZWUujqxUzahzmmJC7s4Xn8Qa2oeh1VB")
                        .Select(p => p.AddressEntity)
                        .Distinct()
                        .ToList();
                //addr = db.Addresses.Where(p => p.ColoredAddress== "akYc7BCwLpf1JWTnQdj8WN94Gajokn8MEhT").ToList();
            }

            var semaphore = new SemaphoreSlim(100);
            var tasksToAwait = new List<Task>();
            var counter = addr.Count();
            var st = new Stopwatch();
            st.Start();
            var mainChain = await mainChainRepository.GetMainChainAsync();
            foreach (var address in addr.Select(p => p.ColoredAddress).Distinct().ToList())
            {
                var balanceId = BalanceIdHelper.Parse(address, Network.Main);
                Console.WriteLine("t");
                var changesTask = indexerClientFactory.GetIndexerClient()
                    .GetConfirmedBalanceChangesAsync(balanceId, mainChain, semaphore, 0, mainChain.Height)
                    .ContinueWith(
                        async task =>
                        {
                            counter--;
                            var counterTemp = counter;
                            Console.WriteLine("Continue started {0} {1}", st.Elapsed.ToString("g"), counter);
                            
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

                            Console.WriteLine(" Continue Done {0} {1} {2}", counterTemp, st.Elapsed.ToString("g"), DateTime.Now.ToString("t"));
                        });

                tasksToAwait.Add(changesTask.Unwrap());
            }

            await Task.WhenAll(tasksToAwait);
            Console.WriteLine("All done");
            Console.ReadLine();
        }
    }
}
