using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Core.Channel;
using Core.Transaction;

namespace Services.Channel
{
    public class FilledChannel:IFilledChannel
    {
        public string OpenTransactionId { get; set; }
        public string CloseTransactionId { get; set; }
        public IOffchainTransaction[] OffchainTransactions { get; set; }
        public ITransaction OpenTransaction { get; set; }
        public ITransaction CloseTransaction { get; set; }

        public static FilledChannel Create(IChannel channel, ITransaction openTransaction,
            ITransaction closeTransaction)
        {
            return new FilledChannel
            {
                OpenTransactionId = channel.OpenTransactionId,
                CloseTransactionId = channel.CloseTransactionId,

                OffchainTransactions = channel.OffchainTransactions,
                CloseTransaction = openTransaction,
                OpenTransaction = openTransaction
            };
        }
    }


    public class ChannelService:IChannelService
    {
        private readonly IChannelRepository _channelRepository;

        private readonly ICachedTransactionService _cachedTransactionService;

        public ChannelService(IChannelRepository channelRepository, ICachedTransactionService cachedTransactionService)
        {
            _channelRepository = channelRepository;
            _cachedTransactionService = cachedTransactionService;
        }

        public async Task<IFilledChannel> GetByOffchainTransactionId(string transactionId)
        {
            var channel = await _channelRepository.GetByOffchainTransactionId(transactionId);

            return await FillChannel(channel);
        }

        public async Task<IEnumerable<IFilledChannel>> GetByBlock(string blockId)
        {
            int height;

            IEnumerable<IChannel> dbChannels;
            if (int.TryParse(blockId, out height))
            {
                dbChannels = await _channelRepository.GetByBlockHeight(height);
            }
            else
            {
                dbChannels = await _channelRepository.GetByBlockId(blockId);
            }

            return await FillChannels(dbChannels);
        }


        private async Task<IFilledChannel> FillChannel(IChannel channel)
        {
            return (await FillChannels(new [] { channel })).First();
        }

        private async Task<IEnumerable<IFilledChannel>> FillChannels(IEnumerable<IChannel> channels)
        {
            var txIds = channels.SelectMany(p => new[] {p.CloseTransactionId, p.OpenTransactionId})
                .Where(p => !string.IsNullOrEmpty(p))
                .Distinct()
                .ToList();

            var txs = (await _cachedTransactionService.GetAsync(txIds)).ToDictionary(p => p.TransactionId);


            return channels.Select(p => FilledChannel.Create(p, txs.GetValueOrDefault(p.OpenTransactionId, null),
                txs.GetValueOrDefault(p.CloseTransactionId, null)));
        }
    }
}
