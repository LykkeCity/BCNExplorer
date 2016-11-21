using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using Core.AssetBlockChanges.Mongo;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace AzureRepositories.AssetCoinHolders
{
    public class AssetBalanceChangesRepository: IAssetBalanceChangesRepository
    {
        private readonly ILog _log;
        private readonly IMongoCollection<AddressAssetBalanceChangeMongoEntity> _mongoCollection;
        private static  SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);
        static AssetBalanceChangesRepository()
        {
            BsonClassMap.RegisterClassMap<AddressAssetBalanceChangeMongoEntity>();
        }
        
        public AssetBalanceChangesRepository(IMongoDatabase db, ILog log)
        {
            _log = log;
            _mongoCollection = db.GetCollection<AddressAssetBalanceChangeMongoEntity>("asset-balance-changes");
        }

        public async Task AddAsync(string coloredAddress, IEnumerable<IBalanceChanges> balanceChanges)
        {
            if (!balanceChanges.Any())
            {
                return;
            }

            const int attemptCountMax = 10;
            var attemptCount = 0;
            bool isDone = false;
            while (!isDone)
            {
                try
                {
                    await _semaphoreSlim.WaitAsync();
                    await AddInnerAsync(coloredAddress, balanceChanges);
                    isDone = true;
                }
                catch (MongoCommandException e)
                {
                    Console.WriteLine("attempt {0}", attemptCount);
                    attemptCount++;
                    //await _log.WriteError("AssetBalanceChangesRepository", "AddAsync", coloredAddress, e);
                    if (attemptCount >= attemptCountMax)
                    {
                        throw;
                    }
                    await Task.Delay(3*1000);
                }
                finally
                {
                    _semaphoreSlim.Release();
                }
            }

        }

        private async Task AddInnerAsync(string coloredAddress, IEnumerable<IBalanceChanges> balanceChanges)
        {
            foreach (var groupedByAssetId in balanceChanges.GroupBy(p => p.AssetId))
            {
                var parsedBlockHashes = await _mongoCollection
                        .Find(p => p.AssetId == groupedByAssetId.Key && p.ColoredAddress == coloredAddress)
                        .Project(p => p.BlockHash).Limit(int.MaxValue).ToListAsync();
                var balanceChangesToInsert =
                    groupedByAssetId.ToList().Where(p => !parsedBlockHashes.Contains(p.BlockHash)).ToList();

                if (balanceChangesToInsert.Any())
                {
                    var blockChangesDictionary = new Dictionary<string, AddressAssetBalanceChangeMongoEntity>();
                    foreach (var balanceChange in balanceChangesToInsert)
                    {
                        AddressAssetBalanceChangeMongoEntity assetBalanceChange;
                        if (blockChangesDictionary.ContainsKey(balanceChange.BlockHash))
                        {
                            assetBalanceChange = blockChangesDictionary[balanceChange.BlockHash];
                        }
                        else
                        {
                            assetBalanceChange = AddressAssetBalanceChangeMongoEntity.Create(assetId: groupedByAssetId.Key, address: coloredAddress, blockHash: balanceChange.BlockHash, blockHeight: balanceChange.BlockHeight);
                            blockChangesDictionary.Add(assetBalanceChange.BlockHash, assetBalanceChange);
                        }

                        assetBalanceChange.BalanceChanges.Add(BalanceChangeMongoEntity.Create(balanceChange.Quantity, balanceChange.TransactionHash));
                    }

                    await _mongoCollection.InsertManyAsync(blockChangesDictionary.Values);
                }
            }
        }

        public async Task<BalanceSummary> GetSummaryAsync(params string[] assetIds)
        {
            var addressBalanceChanges = await _mongoCollection.Find(p => assetIds.Contains(p.AssetId)).ToListAsync();

            return new BalanceSummary
            {
                AssetId = assetIds.FirstOrDefault(),
                AddressSummaries =
                    addressBalanceChanges.GroupBy(p=>p.ColoredAddress).Select(bc => new BalanceSummary.BalanceAddressSummary
                    {
                        Address = bc.Key,
                        Balance = bc.SelectMany(p => p.BalanceChanges).Sum(p => p.Quantity)
                    })
            };
        }

        public async Task<int> GetLastParsedBlockHeightAsync()
        {
            var result =  await _mongoCollection.Find(x => true).SortByDescending(d => d.BlockHeight).Limit(1).FirstOrDefaultAsync();
            return result != null ? result.BlockHeight : 0;
        }
    }

    [BsonIgnoreExtraElements]
    public class AddressAssetBalanceChangeMongoEntity
    {
        public AddressAssetBalanceChangeMongoEntity()
        {
            BalanceChanges = new List<BalanceChangeMongoEntity>();
        }

        public static AddressAssetBalanceChangeMongoEntity Create(string assetId, string address, string blockHash, int blockHeight)

        {
            return new AddressAssetBalanceChangeMongoEntity
            {
                Id = CreateIdKey(assetId, address, blockHash),

                AssetId = assetId,
                ColoredAddress = address,
                BlockHash =  blockHash,
                BlockHeight = blockHeight
            };
        }

        [BsonId]
        public string Id { get; set; }

        [BsonElement("assetid")]
        public string AssetId { get; set; }

        [BsonElement("blockhash")]
        public string BlockHash { get; set; }

        [BsonElement("blockheight")]
        public int BlockHeight { get; set; }

        [BsonElement("address")]
        public string ColoredAddress { get; set; }

        [BsonElement("changes")]
        public List<BalanceChangeMongoEntity> BalanceChanges { get; set; }

        public static string CreateIdKey(string assetId, string address, string blockHash)
        {
            return $"{assetId}_{address}_{blockHash}";
        }
    }

    public class BalanceChangeMongoEntity
    {
        [BsonElement("quantity")]
        public double Quantity { get; set; }

        [BsonElement("txhash")]
        public string TransactionHash { get; set; }

        public static BalanceChangeMongoEntity Create(double quantity, string transactionHash)
        {
            return new BalanceChangeMongoEntity
            {
                Quantity = quantity,
                TransactionHash = transactionHash
            };
        }
    }
}
