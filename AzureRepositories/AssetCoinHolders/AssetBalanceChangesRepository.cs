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

namespace AzureRepositories.AssetCoinHolders
{
    public class AssetBalanceChangesRepository: IAssetBalanceChangesRepository
    {
        private readonly ILog _log;
        private readonly IMongoCollection<AssetBalanceMongoEntity> _mongoCollection;
        private static  SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);
        static AssetBalanceChangesRepository()
        {
            BsonClassMap.RegisterClassMap<AssetBalanceMongoEntity>();
        }
        
        public AssetBalanceChangesRepository(IMongoDatabase db, ILog log)
        {
            _log = log;
            _mongoCollection = db.GetCollection<AssetBalanceMongoEntity>("asset-balances");
        }

        public async Task AddAsync(string coloredAddress, IEnumerable<IBalanceChanges> balanceChanges)
        {
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
            foreach (var balanceChange in balanceChanges.GroupBy(p => p.AssetId))
            {
                var assetBalance = await _mongoCollection.Find(p => p.AssetId == balanceChange.Key).FirstOrDefaultAsync()
                    ?? new AssetBalanceMongoEntity { AssetId = balanceChange.Key };

                var addressBalanceChanges = assetBalance.AddressBalanceChanges
                    .FirstOrDefault(p => p.ColoredAddress == coloredAddress);

                if (addressBalanceChanges == null)
                {
                    addressBalanceChanges = new AddressAssetBalanceChangesMongoEntity { ColoredAddress = coloredAddress };
                    assetBalance.AddressBalanceChanges.Add(addressBalanceChanges);
                }
                var parsedBlockHashes = addressBalanceChanges.BalanceChanges.Select(p => p.BlockHash).Distinct();

                var mappedChanges = balanceChanges.Where(p => !parsedBlockHashes.Contains(p.BlockHash)).Select(BalanceChangesMongoEntity.Create).ToList();
                addressBalanceChanges.BalanceChanges.AddRange(mappedChanges);

                await _mongoCollection.ReplaceOneAsync(p => p.AssetId == balanceChange.Key, assetBalance, new UpdateOptions { IsUpsert = true });
            }
        }

        public async Task<BalanceSummary> GetSummaryAsync(params string[] assetIds)
        {
            var assets = await _mongoCollection.Find(p => assetIds.Contains(p.AssetId)).ToListAsync();

            return new BalanceSummary
            {
                AssetId = assetIds.FirstOrDefault(),
                AddressSummaries =
                    assets.SelectMany(p => p.AddressBalanceChanges).Select(p => new BalanceSummary.BalanceAddressSummary
                    {
                        Address = p.ColoredAddress,
                        Balance = p.BalanceChanges.Sum(bc => bc.Change)
                    })
            };
        }
    }
    

    [BsonIgnoreExtraElements]
    public class AssetBalanceMongoEntity
    {
        public AssetBalanceMongoEntity()
        {
            AddressBalanceChanges = new List<AddressAssetBalanceChangesMongoEntity> ();
        }

        [BsonId]
        public string AssetId { set; get; }
        

        [BsonElement("addresses")]
        public List<AddressAssetBalanceChangesMongoEntity> AddressBalanceChanges { get; set; } 
    }

    public class AddressAssetBalanceChangesMongoEntity
    {
        public AddressAssetBalanceChangesMongoEntity()
        {
            BalanceChanges = new List<BalanceChangesMongoEntity>();
        }

        [BsonElement("assetid")]
        public string AssetId { get; set; }

        [BsonElement("address")]
        public string ColoredAddress { get; set; }

        [BsonElement("balancechanges")]
        public List<BalanceChangesMongoEntity> BalanceChanges { get; set; }
    }

    public class BalanceChangesMongoEntity
    {
        [BsonElement("change")]
        public double Change { get; set; }

        [BsonElement("txhash")]
        public string TransactionHash { get; set; }

        [BsonElement("blockhash")]
        public string BlockHash { get; set; }

        [BsonElement("blockheight")]
        public int BlockHeight { get; set; }

        public static BalanceChangesMongoEntity Create(IBalanceChanges source)
        {
            return new BalanceChangesMongoEntity
            {
                BlockHash = source.BlockHash,
                BlockHeight = source.BlockHeight,
                Change = source.Quantity,
                TransactionHash = source.TransactionHash
            };
        }
    }
}
