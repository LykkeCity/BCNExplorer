using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Transaction;

namespace Core.Channel
{
    public interface IFilledChannel:IChannel
    {
        ITransaction OpenTransaction { get; }
        ITransaction CloseTransaction { get; }
    }
    

    public interface IChannelService
    {
        Task<IFilledChannel> GetByOffchainTransactionIdAsync(string transactionId);
        Task<IEnumerable<IFilledChannel>> GetByBlockAsync(string blockId);
        Task<IEnumerable<IFilledChannel>> GetByAddressPagedAsync(string address, ChannelStatusQueryType channelStatusQueryType = ChannelStatusQueryType.All);
        Task<IEnumerable<IChannel>> GetByAddressAsync(string address, ChannelStatusQueryType channelStatusQueryType = ChannelStatusQueryType.All);
        Task<bool> IsHubAsync(string address);
    }
}
