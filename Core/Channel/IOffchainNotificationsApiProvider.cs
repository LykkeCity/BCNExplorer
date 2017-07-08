using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Channel
{
    public interface IChannel
    {
        string AssetId { get; }
        bool IsColored { get; }
        string OpenTransactionId { get; }

        string CloseTransactionId { get; }

        IEnumerable<IOffchainTransaction> OffchainTransactions { get; }
    }

    public interface IOffchainTransaction
    {
        string TransactionId { get; }

        DateTime DateTime { get; }

        string HubAddress { get; }

        string Address1 { get; }

        string Address2 { get; }

        string AssetId { get; }

        bool IsColored { get; }

        decimal Address1Quantity { get; }

        decimal Address2Quantity { get; }
    }

    public interface IOffchainNotificationsApiProvider
    {
        Task<IChannel> GetByOffchainTransactionIdAsync(string transactionId);
        Task<bool> OffchainTransactionExistsAsync(string transactionId);
        Task<IEnumerable<IChannel>> GetByBlockIdAsync(string blockId, ChannelStatusQueryType channelStatusQueryType = ChannelStatusQueryType.All, IPageOptions pageOptions = null);
        Task<IEnumerable<IChannel>> GetByBlockHeightAsync(int blockHeight, ChannelStatusQueryType channelStatusQueryType = ChannelStatusQueryType.All, IPageOptions pageOptions = null);
        Task<long> GetCountByBlockIdAsync(string blockId);
        Task<long> GetCountByBlockHeightAsync(int blockHeight);
        Task<IEnumerable<IChannel>> GetByAddressAsync(string address, ChannelStatusQueryType channelStatusQueryType = ChannelStatusQueryType.All, IPageOptions pageOptions = null);
        
        Task<bool> IsHubAsync(string address);
        Task<long> GetCountByAddressAsync(string address);
    }

    public enum ChannelStatusQueryType
    {
        All,
        OpenOnly,
        ClosedOnly
    }

    public interface IPageOptions
    {
        int ItemsToSkip { get; }
        int ItemsToTake { get; }
    }

    public class PageOptions : IPageOptions
    {
        private int PageSize { get; set; }
        private int PageNumber { get; set; }

        public int ItemsToSkip => (PageNumber - 1) * PageSize;
        public int ItemsToTake => PageSize;

        public static PageOptions Create(int pageNumber, int pageSize)
        {
            return new PageOptions
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }


    }
}
