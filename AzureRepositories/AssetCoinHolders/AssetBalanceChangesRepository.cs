using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.AssetBlockChanges;
using Core.AssetBlockChanges.Mongo;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace AzureRepositories.AssetCoinHolders
{
    public class AssetBalanceChangesRepository: IAssetBalanceChangesRepository
    {
        private readonly IMongoDatabase _db;

        public AssetBalanceChangesRepository(IMongoDatabase db)
        {
            _db = db;
        }

        public Task AddAsync(IEnumerable<BalanceChange> balanceChanges)
        {
            throw new NotImplementedException();
        }
    }
    

    [BsonIgnoreExtraElements]
    public class AssetBalanceMongoEntity
    {
        [BsonId]
        public string AssetId { set; get; }

        [BsonElement("txid")]
        public string TxId { set; get; }
    }

    public class AddressBalanceChangesMongoEntity
    {
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
    }
}
