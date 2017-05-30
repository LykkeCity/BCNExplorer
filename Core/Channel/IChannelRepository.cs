﻿using System;
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

        IOffchainTransaction[] OffchainTransactions { get; }
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

    public interface IChannelRepository
    {
        Task<IChannel> GetByOffchainTransactionIdAsync(string transactionId);
        Task<IEnumerable<IChannel>> GetByBlockIdAsync(string blockId);
        Task<IEnumerable<IChannel>> GetByBlockHeightAsync(int blockHeight);
        Task<IEnumerable<IChannel>> GetByAddressAsync(string address, ChannelStatusQueryType channelStatusQueryType = ChannelStatusQueryType.All);
        Task<bool> IsHubAsync(string address);
    }

    public enum ChannelStatusQueryType
    {
        All,
        OpenOnly,
        ClosedOnly
    }
}
