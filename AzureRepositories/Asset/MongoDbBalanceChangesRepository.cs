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

    public class BalanceSummary : IBalanceSummary
    {
        public IEnumerable<string> AssetIds { get; set; }
        public IEnumerable<IBalanceAddressSummary> AddressSummaries { get; set; }
    }

    public class BalanceAddressSummary : IBalanceAddressSummary
    {
        public string Address { get; set; }
        public double Balance { get; set; }
    }

    public class BalanceTransaction : IBalanceTransaction
    {
        public string Hash { get; set; }

        public static BalanceTransaction Create(string txHash)
        {
            return new BalanceTransaction
            {
                Hash = txHash
            };
        }
    }

    public class BalanceBlock:IBalanceBlock
    {
        public string Hash { get; set; }
        public int Height { get; set; }

        public static BalanceBlock Create(string hash, int height)
        {
            return new BalanceBlock
            {
                Height = height,
                Hash = hash
            };
        }
    }

    public class MongoDbBalanceChangesRepository: IAssetBalanceChangesRepository
    {
        private readonly ILog _log;
        private readonly IMongoCollection<AddressAssetBalanceChangeMongoEntity> _mongoCollection;
        private static  SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);
        static MongoDbBalanceChangesRepository()
        {
            BsonClassMap.RegisterClassMap<AddressAssetBalanceChangeMongoEntity>();
        }
        
        public MongoDbBalanceChangesRepository(IMongoDatabase db, ILog log)
        {
            _log = log;
            _mongoCollection = db.GetCollection<AddressAssetBalanceChangeMongoEntity>(AddressAssetBalanceChangeMongoEntity.CollectionName);
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
                    attemptCount++;
                    await _log.WriteError("AssetBalanceChangesRepository", "AddAsync", coloredAddress, e);
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

        public Task<IBalanceSummary> GetSummaryAsync(params string[] assetIds)
        {
            return GetSummaryAsync(null, assetIds);
        }

        public async Task<IBalanceSummary> GetSummaryAsync(IQueryOptions queryOptions, params string[] assetIds)
        {
            IFindFluent<AddressAssetBalanceChangeMongoEntity, AddressAssetBalanceChangeMongoEntity> query;
            if (queryOptions != null)
            {
                query = _mongoCollection.Find(p => assetIds.Contains(p.AssetId)
                           && p.BlockHeight <= queryOptions.ToBlockHeight
                           && p.BlockHeight >= queryOptions.FromBlockHeight
                           && p.TotalChanged != 0);
            }
            else
            {
                query= _mongoCollection.Find(p => assetIds.Contains(p.AssetId)
                            && p.TotalChanged != 0);
            }
            
            var addressBalanceChanges = await query.Project(p => new { p.ColoredAddress, Balance = p.TotalChanged }).ToListAsync();
            

            return new BalanceSummary
            {
                AssetIds = assetIds,
                AddressSummaries =
                    addressBalanceChanges.GroupBy(p => p.ColoredAddress).Select(bc => new BalanceAddressSummary
                    {
                        Address = bc.Key,
                        Balance = bc.Sum(p => p.Balance)
                    })
            };
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

                        assetBalanceChange.AddBalanceChanges(BalanceChangeMongoEntity.Create(balanceChange.Quantity, balanceChange.TransactionHash));
                    }

                    await _mongoCollection.InsertManyAsync(blockChangesDictionary.Values);
                }
            }
        }

        public async Task<int> GetLastParsedBlockHeightAsync()
        {
            var result =  await _mongoCollection.Find(x => true).SortByDescending(d => d.BlockHeight).Limit(1).FirstOrDefaultAsync();
            return result != null ? result.BlockHeight : 0;
        }

        public async Task<IEnumerable<IBalanceTransaction>> GetTransactionsAsync(IEnumerable<string> assetIds, int? fromBlock = null)
        {
            IFindFluent<AddressAssetBalanceChangeMongoEntity, AddressAssetBalanceChangeMongoEntity> query;
            if (fromBlock == null)
            {
                query = _mongoCollection.Find(p => assetIds.Contains(p.AssetId));
            }
            else
            {
                query = _mongoCollection.Find(p => assetIds.Contains(p.AssetId) && p.BlockHeight >= fromBlock.Value);
            }

            var queryResult = await query.SortByDescending(p=>p.BlockHeight).Project(p => new { p.BalanceChanges }).ToListAsync();
            var txHashes = queryResult.SelectMany(p => p.BalanceChanges.Select(x => x.TransactionHash));

            return txHashes.Distinct().Select(BalanceTransaction.Create);
        }

        public async Task<IBalanceTransaction> GetLatestTxAsync(IEnumerable<string> assetIds)
        {
            var balanceChanges = (await _mongoCollection.Find(p => assetIds.Contains(p.AssetId))
                .SortByDescending(p => p.BlockHeight)
                .Limit(1)
                .Project(p => p.BalanceChanges)
                .ToListAsync())
                .FirstOrDefault();

            if (balanceChanges != null && balanceChanges.Any())
            {
                return BalanceTransaction.Create(balanceChanges.First().TransactionHash);
            }

            return null;
        }

        public async Task<IDictionary<string, double>> GetAddressQuantityChangesAtBlock(int blockHeight, IEnumerable<string> assetIds)
        {
            var result = await _mongoCollection.Find(
                    p => assetIds.Contains(p.AssetId) && p.BlockHeight == blockHeight && p.TotalChanged != 0)
                    .Project(p => new {p.ColoredAddress, Balance = p.BalanceChanges.Sum(bc => bc.Quantity)})
                    .ToListAsync();

            return result.ToDictionary(p => p.ColoredAddress, p => p.Balance);
        }

        public async Task<IEnumerable<IBalanceBlock>> GetBlocksWithChanges(IEnumerable<string> assetIds)
        {
            var result =
                await
                    _mongoCollection.Find(p => assetIds.Contains(p.AssetId) && p.TotalChanged != 0)
                        .Project(p => new {p.BlockHeight, p.BlockHash})
                        .ToListAsync();

            return result.Distinct().Select(p => BalanceBlock.Create(p.BlockHash, p.BlockHeight));
        }
    }

    [BsonIgnoreExtraElements]
    public class AddressAssetBalanceChangeMongoEntity
    {

        public static string CollectionName => "block-changes";

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

        [BsonElement("totalchanged")]
        public double TotalChanged { get; set; }

        [BsonElement("changes")]
        public List<BalanceChangeMongoEntity> BalanceChanges { get; set; }

        public void AddBalanceChanges(params BalanceChangeMongoEntity[] balanceChanges)
        {
            BalanceChanges.AddRange(balanceChanges);
            TotalChanged = BalanceChanges.Sum(p => p.Quantity);
        }

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
