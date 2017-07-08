using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Channel;
using Core.Settings;
using Flurl;
using Flurl.Http;

namespace Services.Channel
{
    public class Channel : IChannel
    {
        public string AssetId { get; set; }
        public bool IsColored { get; set; }
        public string OpenTransactionId { get; set; }
        public string CloseTransactionId { get; set; }
        public IEnumerable<IOffchainTransaction> OffchainTransactions { get; set; }

        public static Channel Create(ChannelViewModelContract source)
        {
            if (source != null)
            {
                return new Channel
                {
                    AssetId = source.AssetId,
                    CloseTransactionId = source.CloseTransactionId,
                    IsColored = source.IsColored,
                    OpenTransactionId = source.OpenTransactionId,
                    OffchainTransactions = source.OffchainTransactions.Select(OffchainTransaction.Create)
                };
            }

            return null;
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

        public static OffchainTransaction Create(OffchainTransactionViewModelContract source)
        {
            return new OffchainTransaction
            {
                TransactionId = source.TransactionId,
                Address1 = source.Address1,
                Address1Quantity = source.Address1Quantity,
                Address2 = source.Address2,
                Address2Quantity = source.Address2Quantity,
                AssetId = source.AssetId,
                DateTime = source.DateTime,
                HubAddress = source.HubAddress,
                IsColored = source.IsColored
            };
        }
    }

    public class OffchainNotificationsApiProvider : IOffchainNotificationsApiProvider
    {
        private readonly string _baseUrl;

        public OffchainNotificationsApiProvider(BaseSettings baseSettings)
        {
            _baseUrl = baseSettings.OffchainNotificationsHandlerUrl;
        }

        public async Task<IChannel> GetByOffchainTransactionIdAsync(string transactionId)
        {
            var resp = await _baseUrl
                .AppendPathSegment($"api/offchaintransactions/{transactionId}")
                .GetJsonAsync<ChannelViewModelContract>();

            return Channel.Create(resp);
        }

        public async Task<bool> OffchainTransactionExistsAsync(string transactionId)
        {
            var resp = await _baseUrl
                .AppendPathSegment($"api/offchaintransactions/{transactionId}/exists")
                .GetJsonAsync<bool>();

            return resp;
        }

        public async Task<IEnumerable<IChannel>> GetByBlockIdAsync(string blockId, 
            ChannelStatusQueryType channelStatusQueryType = ChannelStatusQueryType.All,
            IPageOptions pageOptions = null)
        {
            var query = _baseUrl
                .AppendPathSegment($"api/channels/blockid/{blockId}");

            query = AppendPageOptions(query, pageOptions);
            query = AppenedChannelStatusQueryType(query, channelStatusQueryType);

            var resp = await query
                .GetJsonAsync<ChannelViewModelContract[]>();

            return resp.Select(Channel.Create);
        }

        public async Task<IEnumerable<IChannel>> GetByBlockHeightAsync(int blockHeight,
            ChannelStatusQueryType channelStatusQueryType = ChannelStatusQueryType.All,
            IPageOptions pageOptions = null)
        {
            var query = _baseUrl
                .AppendPathSegment($"api/channels/height/{blockHeight}");

            query = AppendPageOptions(query, pageOptions);
            query = AppenedChannelStatusQueryType(query, channelStatusQueryType);

            var resp = await query
                .GetJsonAsync<ChannelViewModelContract[]>();

            return resp.Select(Channel.Create);
        }

        public async Task<long> GetCountByBlockIdAsync(string blockId)
        {
            var resp = await _baseUrl
                .AppendPathSegment($"api/channels/blockid/{blockId}/count")
                .GetJsonAsync<long>();

            return resp;
        }

        public async Task<long> GetCountByBlockHeightAsync(int blockHeight)
        {
            var resp = await _baseUrl
                .AppendPathSegment($"api/channels/height/{blockHeight}/count")
                .GetJsonAsync<long>();

            return resp;
        }

        public async Task<IEnumerable<IChannel>> GetByAddressAsync(string address, 
            ChannelStatusQueryType channelStatusQueryType = ChannelStatusQueryType.All,
            IPageOptions pageOptions = null)
        {
            var query = _baseUrl
                .AppendPathSegment($"api/channels/address/{address}");

            query = AppendPageOptions(query, pageOptions);
            query = AppenedChannelStatusQueryType(query, channelStatusQueryType);

            var resp = await query
                .GetJsonAsync<ChannelViewModelContract[]>();

            return resp.Select(Channel.Create);
        }

        public async Task<bool> IsHubAsync(string address)
        {
            var resp = await _baseUrl
                .AppendPathSegment($"api/channels/hub/{address}/exists")
                .GetJsonAsync<bool>();

            return resp;
        }

        public async Task<long> GetCountByAddressAsync(string address)
        {
            var resp = await _baseUrl
                .AppendPathSegment($"api/channels/address/{address}/count")
                .GetJsonAsync<long>();

            return resp;
        }

        private static Url AppendPageOptions(Url source, IPageOptions pageOptions)
        {
            if (pageOptions != null)
            {
                var pageOptionsRequest = new PageOptionsRequestContract
                {
                    Skip = pageOptions.ItemsToSkip,
                    Take = pageOptions.ItemsToTake
                };

                return source.SetQueryParams(pageOptionsRequest);
            }

            return source;
        }

        private static Url AppenedChannelStatusQueryType(Url source, ChannelStatusQueryType type)
        {
            return source.SetQueryParam("channelStatusQueryType", type.ToString());
        }
    }
}
