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
        public string AssetId { get; set; }
        public bool IsColored => AssetId != ChannelMetadataMongoEntity.BtcAssetName;
        public string OpenTransactionId { get; set; }
        public string CloseTransactionId { get; set; }
        public IOffchainTransaction[] OffchainTransactions { get; set; }

        public static Channel Create(ChannelMongoEntity source)
        {
            return new Channel
            {
                OpenTransactionId = source.OpenTransaction?.TransactionId,
                CloseTransactionId = source.CloseTransaction?.TransactionId,
                AssetId = source.Metadata.AssetId,
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

        public async Task<IChannel> GetByOffchainTransactionIdAsync(string transactionId)
        {
            var filterExpression = Builders<ChannelMongoEntity>.Filter.ElemMatch(p => p.OffChainTransactions,
                p => p.TransactionId == transactionId);

            var dbEntity = await _mongoCollection
                .Find(filterExpression)
                .FirstOrDefaultAsync();

            return dbEntity != null ? Channel.Create(dbEntity) : null;
        }

        public async Task<IEnumerable<IChannel>> GetByBlockIdAsync(string blockId, 
            ChannelStatusQueryType channelStatusQueryType = ChannelStatusQueryType.All, 
            IPageOptions pageOptions = null)
        {
            var filterExpression = GetBlockIdFilterExpression(blockId);

            filterExpression = FilterByChannelType(filterExpression, channelStatusQueryType);

            var query = _mongoCollection
                .Find(filterExpression);

            query = PageQuery(query, pageOptions);

            var dbEntities = await query
                .ToListAsync();

            return dbEntities.Select(Channel.Create);
        }

        public async Task<IEnumerable<IChannel>> GetByBlockHeightAsync(int blockHeight, 
            ChannelStatusQueryType channelStatusQueryType = ChannelStatusQueryType.All, 
            IPageOptions pageOptions = null)
        {
            var filterExpression = GetBlockHeightFilterExpression(blockHeight);
            filterExpression = FilterByChannelType(filterExpression, channelStatusQueryType);

            var query = _mongoCollection
                .Find(filterExpression);

            query = PageQuery(query, pageOptions);

            var dbEntities = await query
                .ToListAsync();
            
            return dbEntities.Select(Channel.Create);
        }
        
        public async Task<long> GetCountByBlockIdAsync(string blockId)
        {
            var filterExpression = GetBlockIdFilterExpression(blockId);

            return await _mongoCollection.Find(filterExpression).CountAsync();
        }

        public async Task<long> GetCountByBlockHeightAsync(int blockHeight)
        {
            var filterExpression = GetBlockHeightFilterExpression(blockHeight);

            return await _mongoCollection.Find(filterExpression).CountAsync();
        }


        public async Task<IEnumerable<IChannel>> GetByAddressAsync(string address, 
            ChannelStatusQueryType channelStatusQueryType = ChannelStatusQueryType.All,
            IPageOptions pageOptions = null)
        {
            var filterExpression = GetAddressFilterExpression(address);
            
            filterExpression = FilterByChannelType(filterExpression, channelStatusQueryType);

            var query = _mongoCollection
                .Find(filterExpression);
            
            query = PageQuery(query, pageOptions);

            var dbEntities = await query
                .ToListAsync();

            return dbEntities.Select(Channel.Create);
        }



        public async Task<bool> IsHubAsync(string address)
        {
            var hubAddressFilterExpression = Builders<ChannelMongoEntity>.Filter.Eq(p => p.Metadata.HubAddress, address);

            return await _mongoCollection.Find(hubAddressFilterExpression).CountAsync() > 0;
        }

        public async Task<long> GetCountByAddressAsync(string address)
        {
            var filterExpression = GetAddressFilterExpression(address);

            return await _mongoCollection
                .Find(filterExpression)
                .CountAsync();
        }

        private FilterDefinition<ChannelMongoEntity> FilterByChannelType(
            FilterDefinition<ChannelMongoEntity> filterExpression, ChannelStatusQueryType channelStatusQueryType)
        {
            if (channelStatusQueryType == ChannelStatusQueryType.OpenOnly)
            {
                var openFilterExpession = Builders<ChannelMongoEntity>.Filter.Exists(p => p.CloseTransaction.TransactionId, false);
                return Builders<ChannelMongoEntity>.Filter.And(filterExpression, openFilterExpession);
            }
            else if (channelStatusQueryType == ChannelStatusQueryType.ClosedOnly)
            {
                var closedFilterExpression = Builders<ChannelMongoEntity>.Filter.Exists(p => p.CloseTransaction.TransactionId, true);
                return Builders<ChannelMongoEntity>.Filter.And(filterExpression, closedFilterExpression);
            }

            return filterExpression;
        }

        private FilterDefinition<ChannelMongoEntity> GetAddressFilterExpression(string address)
        {
            var hubAddressFilterExpression = Builders<ChannelMongoEntity>.Filter.Eq(p => p.Metadata.HubAddress, address);
            var address1filterExpression = Builders<ChannelMongoEntity>.Filter.Eq(p => p.Metadata.ClientAddress1, address);
            var address2FilterExpression = Builders<ChannelMongoEntity>.Filter.Eq(p => p.Metadata.ClientAddress2, address);

            var filterExpression = Builders<ChannelMongoEntity>.Filter.Or(hubAddressFilterExpression,
                address1filterExpression,
                address2FilterExpression);

            return filterExpression;
        }

        private FilterDefinition<ChannelMongoEntity> GetBlockIdFilterExpression(string blockId)
        {
            var openTxEqualsfilterExpression = Builders<ChannelMongoEntity>.Filter.Eq(p => p.OpenTransaction.Block.BlockId, blockId);

            var closeTxEqualsfilterExpression = Builders<ChannelMongoEntity>.Filter.Eq(p => p.CloseTransaction.Block.BlockId, blockId);

            return Builders<ChannelMongoEntity>.Filter.Or(openTxEqualsfilterExpression, closeTxEqualsfilterExpression);
        }


        private FilterDefinition<ChannelMongoEntity> GetBlockHeightFilterExpression(int blockHeight)
        {
            var openTxEqualsfilterExpression = Builders<ChannelMongoEntity>.Filter.Eq(p => p.OpenTransaction.Block.Height, blockHeight);

            var closeTxEqualsfilterExpression = Builders<ChannelMongoEntity>.Filter.Eq(p => p.CloseTransaction.Block.Height, blockHeight);

            return Builders<ChannelMongoEntity>.Filter.Or(openTxEqualsfilterExpression, closeTxEqualsfilterExpression);
        }


        private IFindFluent<ChannelMongoEntity, ChannelMongoEntity> PageQuery(IFindFluent<ChannelMongoEntity, ChannelMongoEntity> query,
            IPageOptions pageOptions)
        {
            if (pageOptions != null)
            {
                query = query.Skip(pageOptions.ItemsToSkip).Limit(pageOptions.ItemsToTake);
            }

            return query;
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
