using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Common.Log;
using Core.AssetBlockChanges.Mongo;
using Core.Settings;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using AzureStorage.DocDb;

namespace AzureRepositories.Asset
{
    public class DocumentDbBalanceChangesRepository : IAssetBalanceChangesRepository
    {
        private readonly ILog _log;
        private readonly string _databaseName;
        private readonly BaseSettings _baseSettings;

        public DocumentDbBalanceChangesRepository(BaseSettings baseSettings, ILog log)
        {
            _baseSettings = baseSettings;

            _databaseName = baseSettings.Db.AssetBalanceChangesDocumentDb.DbName;
            _log = log;
        }

        private DocumentClient CreateDocumentClient()
        {
            return new DocumentClient(new Uri(_baseSettings.Db.AssetBalanceChangesDocumentDb.EndpointUri),
                _baseSettings.Db.AssetBalanceChangesDocumentDb.PrimaryKey);
        }

        private async Task<DocumentCollection> GetCollectionAsync(DocumentClient docClient)
        {
            try
            {
                return await docClient.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(_databaseName, AddressAssetBalanceChangeDocDbEntity.CollectionName));
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await
                        _log.WriteWarning("DocumentDbBalanceChangesRepository", "GetCollectionAsync", null,
                            "Creating collection started");

                    await docClient.CreateDatabaseIfNotExistsAsync(new Database { Id = _databaseName });

                    var collectionSpec = new DocumentCollection
                    {
                        Id = AddressAssetBalanceChangeDocDbEntity.CollectionName
                    };

                    await docClient.CreateDocumentCollectionIfNotExistsAsync(
                            UriFactory.CreateDatabaseUri(_databaseName), collectionSpec);

                    //TODO create aggregated stored procedures

                    var result = await docClient.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(_databaseName, AddressAssetBalanceChangeDocDbEntity.CollectionName));

                    await _log.WriteWarning("DocumentDbBalanceChangesRepository", "GetCollectionAsync", null, "Creating collection done");

                    return result;
                }
                else
                {
                    throw;
                }
            }
        } 

        public async Task AddAsync(string coloredAddress, IEnumerable<IBalanceChanges> balanceChanges)
        {
            using (var client = CreateDocumentClient())
            {
                var collection = await GetCollectionAsync(client);
                foreach (var groupedByAssetId in balanceChanges.GroupBy(p => p.AssetId))
                {
                       var parsedBlockHashes = await  client.CreateDocumentQuery<AddressAssetBalanceChangeDocDbEntity>(collection.DocumentsLink)
                            .Where(p => p.AssetId == groupedByAssetId.Key && p.ColoredAddress == coloredAddress)
                            .Select(p => p.BlockHash)
                            .AsDocumentQuery()
                            .QueryAsync();
                    
                    var balanceChangesToInsert =
                        groupedByAssetId.ToList().Where(p => !parsedBlockHashes.Contains(p.BlockHash)).ToList();

                    if (balanceChangesToInsert.Any())
                    {
                        var blockChangesDictionary = new Dictionary<string, AddressAssetBalanceChangeDocDbEntity>();
                        foreach (var balanceChange in balanceChangesToInsert)
                        {
                            AddressAssetBalanceChangeDocDbEntity assetBalanceChange;
                            if (blockChangesDictionary.ContainsKey(balanceChange.BlockHash))
                            {
                                assetBalanceChange = blockChangesDictionary[balanceChange.BlockHash];
                            }
                            else
                            {
                                assetBalanceChange = AddressAssetBalanceChangeDocDbEntity.Create(assetId: groupedByAssetId.Key, address: coloredAddress, blockHash: balanceChange.BlockHash, blockHeight: balanceChange.BlockHeight);
                                blockChangesDictionary.Add(assetBalanceChange.BlockHash, assetBalanceChange);
                            }

                            assetBalanceChange.AddBalanceChanges(BalanceChangeDocDbEntity.Create(balanceChange.Quantity, balanceChange.TransactionHash));
                        }
                        
                        var insertTasks = new List<Task>();

                        foreach (var doc in blockChangesDictionary.Values)
                        {
                            var task = client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(_databaseName, AddressAssetBalanceChangeDocDbEntity.CollectionName), doc);
                            insertTasks.Add(task);
                        }

                        await Task.WhenAll(insertTasks);
                    }
                }
            }

        }

        public Task<IBalanceSummary> GetSummaryAsync(params string[] assetIds)
        {
            throw new NotImplementedException();
        }

        public Task<IBalanceSummary> GetSummaryAsync(IQueryOptions queryOptions, params string[] assetIds)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetLastParsedBlockHeightAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IBalanceTransaction>> GetTransactionsAsync(IEnumerable<string> assetIds, int? fromBlock = null)
        {
            throw new NotImplementedException();
        }

        public Task<IBalanceTransaction> GetLatestTxAsync(IEnumerable<string> assetIds)
        {
            throw new NotImplementedException();
        }

        public Task<IDictionary<string, double>> GetAddressQuantityChangesAtBlock(int blockHeight, IEnumerable<string> assetIds)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IBalanceBlock>> GetBlocksWithChanges(IEnumerable<string> assetIds)
        {
            throw new NotImplementedException();
        }
    }
    
    public class AddressAssetBalanceChangeDocDbEntity
    {

        public static string CollectionName => "block-changes";

        public AddressAssetBalanceChangeDocDbEntity()
        {
            BalanceChanges = new List<BalanceChangeDocDbEntity>();
        }

        public static AddressAssetBalanceChangeDocDbEntity Create(string assetId, string address, string blockHash, int blockHeight)

        {
            return new AddressAssetBalanceChangeDocDbEntity
            {
                Id = CreateIdKey(assetId, address, blockHash),

                AssetId = assetId,
                ColoredAddress = address,
                BlockHash = blockHash,
                BlockHeight = blockHeight
            };
        }

        public string Id { get; set; }
        
        public string AssetId { get; set; }
        
        public string BlockHash { get; set; }
        
        public int BlockHeight { get; set; }
        
        public string ColoredAddress { get; set; }
        
        public double TotalChanged { get; set; }
        
        public List<BalanceChangeDocDbEntity> BalanceChanges { get; set; }

        public void AddBalanceChanges(params BalanceChangeDocDbEntity[] balanceChanges)
        {
            BalanceChanges.AddRange(balanceChanges);
            TotalChanged = BalanceChanges.Sum(p => p.Quantity);
        }

        public static string CreateIdKey(string assetId, string address, string blockHash)
        {
            return $"{assetId}_{address}_{blockHash}";
        }
    }

    public class BalanceChangeDocDbEntity
    {
        public double Quantity { get; set; }
        
        public string TransactionHash { get; set; }

        public static BalanceChangeDocDbEntity Create(double quantity, string transactionHash)
        {
            return new BalanceChangeDocDbEntity
            {
                Quantity = quantity,
                TransactionHash = transactionHash
            };
        }
    }
}
