using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Core.Channel;
using Core.Settings;
using Core.Transaction;
using NBitcoin;
using Providers.Helpers;

namespace Services.Channel
{
    public class FilledChannel:IFilledChannel
    {
        public string AssetId { get; set; }
        public bool IsColored { get; set; }
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
                CloseTransaction = closeTransaction,
                OpenTransaction = openTransaction,
                AssetId = channel.AssetId,
                IsColored = channel.IsColored
            };
        }
    }


    public class ChannelService:IChannelService
    {
        private readonly IChannelRepository _channelRepository;
        private readonly BaseSettings _baseSettings;
        private readonly ICachedTransactionService _cachedTransactionService;

        public ChannelService(IChannelRepository channelRepository, ICachedTransactionService cachedTransactionService, BaseSettings baseSettings)
        {
            _channelRepository = channelRepository;
            _cachedTransactionService = cachedTransactionService;
            _baseSettings = baseSettings;
        }

        public async Task<IFilledChannel> GetByOffchainTransactionIdAsync(string transactionId)
        {
            var channel = await _channelRepository.GetByOffchainTransactionIdAsync(transactionId);

            return await FillChannel(channel);
        }

        public async Task<IEnumerable<IFilledChannel>> GetByBlockAsync(string blockId)
        {
            int height;

            IEnumerable<IChannel> dbChannels;
            if (int.TryParse(blockId, out height))
            {
                dbChannels = await _channelRepository.GetByBlockHeightAsync(height);
            }
            else
            {
                dbChannels = await _channelRepository.GetByBlockIdAsync(blockId);
            }

            return await FillChannels(dbChannels);
        }

        public async Task<IEnumerable<IFilledChannel>> GetByAddressPagedAsync(string address, ChannelStatusQueryType channelStatusQueryType = ChannelStatusQueryType.All)
        {
            var uncoloredAddress = GetUncoloredAddress(address);

            var dbChannels = await _channelRepository.GetByAddressAsync(uncoloredAddress, channelStatusQueryType);

            return await FillChannels(dbChannels);
        }

        public async Task<IEnumerable<IChannel>> GetByAddressAsync(string address, ChannelStatusQueryType channelStatusQueryType = ChannelStatusQueryType.All)
        {
            var uncoloredAddress = GetUncoloredAddress(address);

            return await _channelRepository.GetByAddressAsync(uncoloredAddress, channelStatusQueryType);
        }

        public Task<bool> IsHubAsync(string address)
        {
            var uncoloredAddress = GetUncoloredAddress(address);

            return _channelRepository.IsHubAsync(uncoloredAddress);
        }

        private string GetUncoloredAddress(string address)
        {
            if (BitcoinAddressHelper.IsBitcoinColoredAddress(address, _baseSettings.UsedNetwork()))
            {
                var coloredAddress= new BitcoinColoredAddress(address, _baseSettings.UsedNetwork());

                return coloredAddress.Address.ToWif();
            }

            return address;
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
            
            return channels.Select(p => 
            FilledChannel.Create(p, 
                p.OpenTransactionId != null ? txs.GetValueOrDefault(p.OpenTransactionId, null): null,
                p.CloseTransactionId != null ? txs.GetValueOrDefault(p.CloseTransactionId, null): null));
        }
    }
}
