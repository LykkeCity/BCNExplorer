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
        Task<IFilledChannel> GetByOffchainTransactionId(string transactionId);
        Task<IEnumerable<IFilledChannel>> GetByBlock(string blockId);
        Task<IEnumerable<IFilledChannel>> GetByAddress(string address);
    }
}
