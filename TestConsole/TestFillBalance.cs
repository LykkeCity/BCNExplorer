﻿using System;
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




            var asset = "ASzmrSxhHjioWMYivoawap9yY4cxAfAMxR";

            Console.WriteLine("Getting Coinprism addresses");
            var coinprismAddresses = (await GetAddressesWithColoredAssets.GetAddresses(asset)).ToArray();
            Console.WriteLine("Getting Coinprism Done");
            Console.WriteLine("Saving addresses");
            await addressRepo.AddAsync(coinprismAddresses);
            Console.WriteLine("Saving addresses done");
            var addresses = (await addressRepo.GetAllAsync()).OrderBy(p => p.LegacyAddress).ToArray();

            var legacyAddresses = coinprismAddresses.Select(x => x.LegacyAddress).ToArray();
            addresses = addresses.Where(p => legacyAddresses.Contains(p.LegacyAddress)).ToArray();

            var mainChain = mainChainRepository.GetMainChainAsync().Result;

            var semaphore = new SemaphoreSlim(100);

            var counter = addresses.Count();

            var tasksToAwait = new List<Task>();
            var st = new Stopwatch();
            st.Start();
            Console.WriteLine("Retrievingbalances");
            foreach (var address in addresses)
            {
                var balanceId = BalanceIdHelper.Parse(address.LegacyAddress, Network.Main);

                var changesTask = indexerClient.GetConfirmedBalanceChangesAsync(balanceId, mainChain, semaphore, 0, mainChain.Height).ContinueWith(
                    async task =>
                    {
                        try
                        {
                            if (address.LegacyAddress == "38DdqhVuqb36jjmxTbvBkiFPXKA8EmW9Ly")
                            {
                                
                            }
                            counter--;
                            Console.WriteLine("{0} {1}", st.Elapsed.ToString("g"), counter);

                            var coloredChanges = task.Result.SelectMany(p => p.GetColoredChanges(Network.Main)).ToList();

                            var blocks =
                                coloredChanges.Select(p => p.BlockHash).Select(p => new Core.AssetBlockChanges.Block
                                {
                                    Hash = p,
                                    Height = mainChain.GetBlock(uint256.Parse(p)).Height
                                }).ToArray();

                            await blockRepo.AddAsync(blocks);

                            var transactions = coloredChanges.Select(p => new Core.AssetBlockChanges.Transaction
                            {
                                Hash = p.TransactionHash,
                                BlockHash = p.BlockHash
                            }).ToArray();

                            await transactionRepo.AddAsync(transactions);

                            var balanceChanges = coloredChanges.Select(p => new BalanceChange
                            {
                                AssetId = p.AssetId,
                                Change = p.Quantity,
                                TransactionHash = p.TransactionHash,
                                Address = address.LegacyAddress,
                                BlockHash = p.BlockHash
                            }).ToArray();


                            await changesRepo.AddAsync(address.LegacyAddress, balanceChanges);
                        }
                        catch (Exception e )
                        {
                            Console.WriteLine(e.ToString());
                        }
                    });

                tasksToAwait.Add(changesTask);
            }

            await Task.WhenAll(tasksToAwait);



            Console.WriteLine("All done {0}", st.Elapsed.ToString("g"));
            Console.ReadLine();
        }
    }
}
