using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Channel
{

    
    public interface IChannel
    {
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
        Task<IChannel> GetByOffchainTransactionId(string transactionId);
        Task<IEnumerable<IChannel>> GetByBlockId(string blockId);
        Task<IEnumerable<IChannel>> GetByBlockHeight(int blockHeight);
    }
}
