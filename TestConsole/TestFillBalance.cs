using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.IocContainer;
using Core.AssetBlockChanges;
using JobsCommon;
using NBitcoin;
using NBitcoin.Indexer;
using Providers.Helpers;
using SQLRepositories.Context;
using SQLRepositories.DbModels;
using IBlockRepository = Core.AssetBlockChanges.IBlockRepository;
using ITransactionRepository = Core.AssetBlockChanges.ITransactionRepository;

namespace TestConsole
{
    public class TestFillBalance
    {
        public static async Task Run(IoC ioc)
        {
            var addressRepo = ioc.GetObject<IAddressRepository>();
            var blockRepo = ioc.GetObject<IBlockRepository>();
            var transactionRepo = ioc.GetObject<ITransactionRepository>();
            var changesRepo = ioc.GetObject<IBalanceChangesRepository>();
            var indexerClient = ioc.GetObject<IndexerClient>();
            var mainChainRepository = ioc.GetObject<MainChainRepository>();
            var contextFactory = ioc.GetObject<BcnExplolerFactory>();




            var asset = "AKi5F8zPm7Vn1FhLqQhvLdoWNvWqtwEaig";

            Console.WriteLine("Getting Coinprism addresses");
            var coinprismAddresses = (await GetAddressesWithColoredAssets.GetAddresses(asset)).ToArray();
            Console.WriteLine("Getting Coinprism Done");
            Console.WriteLine("Saving addresses");
            await addressRepo.AddAsync(coinprismAddresses);
            Console.WriteLine("Saving addresses done");
            var addresses = (await addressRepo.GetAllAsync()).OrderBy(p => p.ColoredAddress).ToArray();

            var legacyAddresses = coinprismAddresses.Select(x => x.ColoredAddress).ToArray();
            addresses = addresses.Where(p => legacyAddresses.Contains(p.ColoredAddress)).ToArray();

            var mainChain = mainChainRepository.GetMainChainAsync().Result;

            var semaphore = new SemaphoreSlim(100);

            var counter = addresses.Count();

            var tasksToAwait = new List<Task>();
            var st = new Stopwatch();
            st.Start();
            Console.WriteLine("Retrievingbalances");
            foreach (var address in addresses)
            {
                var balanceId = BalanceIdHelper.Parse(address.ColoredAddress, Network.Main);

                //var changesTask = indexerClient.GetConfirmedBalanceChangesAsync(balanceId, mainChain, semaphore, 0, mainChain.Height).ContinueWith(
                //    async task =>
                //    {
                //        try
                //        {
                //            counter--;
                //            Console.WriteLine("{0} {1}", st.Elapsed.ToString("g"), counter);

                //            var coloredChanges = task.Result.SelectMany(p => p.GetColoredChanges(Network.Main)).ToList();

                //            var blocks =
                //                coloredChanges.Select(p => p.BlockHash).Select(p => new Core.AssetBlockChanges.Block
                //                {
                //                    Hash = p,
                //                    Height = mainChain.GetBlock(uint256.Parse(p)).Height
                //                }).ToArray();

                //            await blockRepo.AddAsync(blocks);

                //            var transactions = coloredChanges.Select(p => new Core.AssetBlockChanges.Transaction
                //            {
                //                Hash = p.TransactionHash,
                //                BlockHash = p.BlockHash
                //            }).ToArray();

                //            await transactionRepo.AddAsync(transactions);

                //            var balanceChanges = coloredChanges.Select(p => new BalanceChange
                //            {
                //                AssetId = p.AssetId,
                //                Change = p.Quantity,
                //                TransactionHash = p.TransactionHash,
                //                Address = address.ColoredAddress,
                //                BlockHash = p.BlockHash
                //            }).ToArray();


                //            await changesRepo.AddAsync(address.ColoredAddress, balanceChanges);
                //            Console.WriteLine("Done");
                //        }
                //        catch (Exception e)
                //        {
                //            Console.WriteLine(e.ToString());
                //        }
                //    });

                //tasksToAwait.Add(changesTask.Unwrap());




                counter--;

                if (counter == 14)
                {

                }
                Console.WriteLine("started {0} {1}", st.Elapsed.ToString("g"), counter);
                var balance =
                    await
                        indexerClient.GetConfirmedBalanceChangesAsync(balanceId, mainChain, semaphore, 0,
                            mainChain.Height);
                var coloredChanges = balance.SelectMany(p => p.GetColoredChanges(Network.Main)).ToList();

                var blocks =
                    coloredChanges.Select(p => p.BlockHash).Select(p => new Core.AssetBlockChanges.Block
                    {
                        Hash = p,
                        Height = mainChain.GetBlock(uint256.Parse(p)).Height
                    }).ToArray();

                Console.WriteLine("Saving blocks");
                await blockRepo.AddAsync(blocks);

                var transactions = coloredChanges.Select(p => new Core.AssetBlockChanges.Transaction
                {
                    Hash = p.TransactionHash,
                    BlockHash = p.BlockHash
                }).ToArray();

                Console.WriteLine("Saving transactions");
                await transactionRepo.AddAsync(transactions);

                var balanceChanges = coloredChanges.Select(p => new BalanceChange
                {
                    AssetId = p.AssetId,
                    Change = p.Quantity,
                    TransactionHash = p.TransactionHash,
                    Address = address.ColoredAddress,
                    BlockHash = p.BlockHash
                }).ToArray();


                Console.WriteLine("Saving balanceChanges");
                await changesRepo.AddAsync(address.ColoredAddress, balanceChanges);
                Console.WriteLine("Done");
            }

            await Task.WhenAll(tasksToAwait);

            //foreach (var task in tasksToAwait)
            //{
            //    Console.WriteLine(task.Status);
            //}

            Console.WriteLine("All done {0}", st.Elapsed.ToString("g"));
            Console.ReadLine();
        }
    }
}
