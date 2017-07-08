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
        public IEnumerable<IOffchainTransaction> OffchainTransactions { get; set; }
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
        private readonly IOffchainNotificationsApiProvider _offchainNotificationsApiProvider;
        private readonly BaseSettings _baseSettings;
        private readonly ICachedTransactionService _cachedTransactionService;

        public ChannelService(IOffchainNotificationsApiProvider offchainNotificationsApiProvider, ICachedTransactionService cachedTransactionService, BaseSettings baseSettings)
        {
            _offchainNotificationsApiProvider = offchainNotificationsApiProvider;
            _cachedTransactionService = cachedTransactionService;
            _baseSettings = baseSettings;
        }

        public async Task<IFilledChannel> GetByOffchainTransactionIdAsync(string transactionId)
        {
            var channel = await _offchainNotificationsApiProvider.GetByOffchainTransactionIdAsync(transactionId);

            return await FillChannel(channel);
        }

        public Task<bool> OffchainTransactionExistsAsync(string transactionId)
        {
            return _offchainNotificationsApiProvider.OffchainTransactionExistsAsync(transactionId);
        }

        public async Task<IEnumerable<IFilledChannel>> GetByBlockAsync(string blockId,
            ChannelStatusQueryType channelStatusQueryType = ChannelStatusQueryType.All,
            IPageOptions pageOptions = null)
        {
            int height;

            IEnumerable<IChannel> dbChannels;
            if (int.TryParse(blockId, out height))
            {
                dbChannels = await _offchainNotificationsApiProvider.GetByBlockHeightAsync(height, 
                    channelStatusQueryType, 
                    pageOptions);
            }
            else
            {
                dbChannels = await _offchainNotificationsApiProvider.GetByBlockIdAsync(blockId, 
                    channelStatusQueryType, 
                    pageOptions);
            }

            return await FillChannels(dbChannels);
        }

        public async Task<long> GetCountByBlockAsync(string blockId)
        {
            int height;
            
            if (int.TryParse(blockId, out height))
            {
                return await _offchainNotificationsApiProvider.GetCountByBlockHeightAsync(height);
            }
            else
            {
                return await _offchainNotificationsApiProvider.GetCountByBlockIdAsync(blockId);
            }
        }

        public async Task<IEnumerable<IFilledChannel>> GetByAddressFilledAsync(string address, 
            ChannelStatusQueryType channelStatusQueryType = ChannelStatusQueryType.All,
            IPageOptions pageOptions = null)
        {
            var uncoloredAddress = GetUncoloredAddress(address);

            var dbChannels = await _offchainNotificationsApiProvider.GetByAddressAsync(uncoloredAddress, channelStatusQueryType, pageOptions);

            return await FillChannels(dbChannels);
        }
        
        public Task<long> GetCountByAddressAsync(string address)
        {
            var uncoloredAddress = GetUncoloredAddress(address);

            return _offchainNotificationsApiProvider.GetCountByAddressAsync(uncoloredAddress);
        }

        public async Task<IEnumerable<IChannel>> GetByAddressAsync(string address, ChannelStatusQueryType channelStatusQueryType = ChannelStatusQueryType.All)
        {
            var uncoloredAddress = GetUncoloredAddress(address);

            return await _offchainNotificationsApiProvider.GetByAddressAsync(uncoloredAddress, channelStatusQueryType);
        }

        public Task<bool> IsHubAsync(string address)
        {
            var uncoloredAddress = GetUncoloredAddress(address);

            return _offchainNotificationsApiProvider.IsHubAsync(uncoloredAddress);
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
