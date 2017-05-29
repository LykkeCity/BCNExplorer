using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        Task<IEnumerable<IFilledChannel>> GetByAddressAsync(string address);
        Task<bool> IsHubAsync(string address);
    }
}
