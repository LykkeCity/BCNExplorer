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
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using NBitcoin;
using Providers;
using Providers.Helpers;
using Services.MainChain;

namespace TestConsole
{
    public class DocDbFillBalance
    {
        public static async Task Run(IoC ioc)
        {
            var balanceChangesRepo = ioc.GetObject<IAssetBalanceChangesRepository>();
            var indexerClientFactory = ioc.GetObject<IndexerClientFactory>();
            var baseSettings = ioc.GetObject<BaseSettings>();
            var log = ioc.GetObject<ILog>();

            var mainChainRepository = ioc.GetObject<MainChainRepository>();

            var collection =
                new MongoClient(baseSettings.Db.AssetBalanceChanges.ConnectionString)
                .GetDatabase(baseSettings.Db.AssetBalanceChanges.DbName)
                .GetCollection<AddressAssetBalanceChangeMongoEntity>(AddressAssetBalanceChangeMongoEntity.CollectionName);

            var parsedAddressesCollection = new MongoClient(baseSettings.Db.AssetBalanceChanges.ConnectionString)
                .GetDatabase(baseSettings.Db.AssetBalanceChanges.DbName)
                .GetCollection<ParsedAddressMongoDbEntity>("parsed-addresses-doc-db");

            Start:
            var parsedAddresses =
                (await parsedAddressesCollection.Find(p => true).Project(p => p.Address).Limit(int.MaxValue).ToListAsync()).Distinct().ToDictionary(p => p);
            var allAddresses =
                (await collection.Find(p => true).Project(p => p.ColoredAddress).Limit(int.MaxValue).ToListAsync()).Distinct().ToDictionary(p => p);

            var addr = allAddresses.Where(p => !parsedAddresses.ContainsKey(p.Value));
            //await parsedAddressesCollection.InsertManyAsync(parsedAddresses.Values.Select(p=> new ParsedAddressMongoDbEntity(p)));

            Console.WriteLine("parsedAddresses " + parsedAddresses.Count);

            var file = "./errors.txt";

            var tasksToAwait = new List<Task>();
            var counter = addr.Count();
            var st = new Stopwatch();
            st.Start();
            var mainChain = await mainChainRepository.GetMainChainAsync();
            var to = mainChain.Height;
            to = 439808;
            var from = 274250;


            File.AppendAllLines(file, new[] { to.ToString(), "----------------" });
            Console.WriteLine(addr.Count());
            foreach (var address in addr.Select(p => p.Value).ToList())
            {
                var t = (await parsedAddressesCollection.Find(p => p.Address == address).Project(p => p.Address).Limit(int.MaxValue).ToListAsync()).Distinct().ToDictionary(p => p);
                if (t.Count != 0)
                {
                    break;

                }
                var balanceId = BalanceIdHelper.Parse(address, Network.Main);
                Console.WriteLine(address + " " + t.Count);
                var changesTask = indexerClientFactory.GetIndexerClient()
                    .GetConfirmedBalanceChangesAsync(balanceId, mainChain, from, to)
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
                                await parsedAddressesCollection.InsertOneAsync(new ParsedAddressMongoDbEntity(address));
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

public class ParsedAddressMongoDbEntity
{
    public ParsedAddressMongoDbEntity(string adddr)
    {
        Address = adddr;
    }

    [BsonId]
    public string Address { get; set; }
}
