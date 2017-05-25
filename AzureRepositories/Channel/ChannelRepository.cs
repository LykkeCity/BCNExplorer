using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Channel;
using Core.Settings;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace AzureRepositories.Channel
{
    public class Channel : IChannel
    {
        public string OpenTransactionId { get; set; }
        public string CloseTransactionId { get; set; }
        public IOffchainTransaction[] OffchainTransactions { get; set; }

        public static Channel Create(ChannelMongoEntity source)
        {
            return new Channel
            {
                OpenTransactionId = source.OpenTransaction?.TransactionId,
                CloseTransactionId = source.CloseTransaction?.TransactionId,
                OffchainTransactions = source.OffChainTransactions.Select(p => OffchainTransaction.Create(p, source.Metadata)).ToArray()
            };
        }
    }

    public class OffchainTransaction: IOffchainTransaction
    {
        public string TransactionId { get; set; }
        public DateTime DateTime { get; set; }
        public string HubAddress { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string AssetId { get; set; }
        public bool IsColored { get; set; }
        public decimal Address1Quantity { get; set; }
        public decimal Address2Quantity { get; set; }

        public static OffchainTransaction Create(OffChainTransactionMongoEntity transaction,
            ChannelMetadataMongoEntity metadata)
        {
            return new OffchainTransaction
            {
                TransactionId = transaction.TransactionId,
                Address1 = metadata.ClientAddress1,
                Address2 = metadata.ClientAddress2,
                HubAddress = metadata.HubAddress,
                Address1Quantity = transaction.ClientAddress1Quantity,
                Address2Quantity = transaction.ClientAddress2Quantity,
                AssetId = metadata.AssetId,
                IsColored = metadata.AssetId != ChannelMetadataMongoEntity.BtcAssetName,
                DateTime = transaction.TimeAdd
            };
        }
    }

    public class ChannelRepository:IChannelRepository
    {
        private readonly IMongoCollection<ChannelMongoEntity> _mongoCollection;

        static ChannelRepository()
        {
            BsonClassMap.RegisterClassMap<ChannelMongoEntity>();
        }
        public ChannelRepository(BaseSettings baseSettings)
        {
            _mongoCollection = new MongoClient(baseSettings.Db.Offchain.ConnectionString)
                .GetDatabase(baseSettings.Db.Offchain.DbName)
                .GetCollection<ChannelMongoEntity>(ChannelMongoEntity.CollectionName);
        }

        public async Task<IChannel> GetByOffchainTransactionId(string transactionId)
        {
            var filterExpression = Builders<ChannelMongoEntity>.Filter.ElemMatch(p => p.OffChainTransactions,
                p => p.TransactionId == transactionId);

            var dbEntity = await _mongoCollection
                .Find(filterExpression)
                .FirstOrDefaultAsync();

            return dbEntity != null ? Channel.Create(dbEntity) : null;
        }
    }

    public class ChannelMongoEntity
    {
        public const string CollectionName = "channels";

        [BsonId]
        public string Id { get; set; }
        public string ChannelId { get; set; }

        public ChannelMetadataMongoEntity Metadata { get; set; }
        public OnChainTransactionMongoEntity OpenTransaction { get; set; }
        public OnChainTransactionMongoEntity CloseTransaction { get; set; }

        public OffChainTransactionMongoEntity[] OffChainTransactions { get; set; }
    }


    public class ChannelMetadataMongoEntity
    {
        public const string BtcAssetName = "BTC";

        public string AssetId { get; set; }
        public string HubAddress { get; set; }
        public string ClientAddress1 { get; set; }
        public string ClientAddress2 { get; set; }
    }

    public class OnChainTransactionMongoEntity
    {

        public string TransactionId { get; set; }

        public BlockMongoEntity Block { get; set; }

        public bool Broadcasted { get; set; }
    }

    public class BlockMongoEntity
    {
        public DateTime Time { get; set; }

        public int Height { get; set; }

        public string BlockId { get; set; }
    }

    public class OffChainTransactionMongoEntity
    {
        public string TransactionId { get; set; }

        public decimal ClientAddress1Quantity { get; set; }

        public decimal ClientAddress2Quantity { get; set; }

        public DateTime TimeAdd { get; set; }
    }
}
