using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.AddressService
{
    public interface IAddressBalance
    {
        string AddressId { get; }
        IEnumerable<IAddressTransaction> AllTransactionIds { get; }
        IEnumerable<IAddressTransaction> SendTransactionIds { get; }
        IEnumerable<IAddressTransaction> ReceivedTransactionIds { get; }
        int TotalTransactions { get; }
        string UncoloredAddress { get; }
        string ColoredAddress { get; }
        double Balance { get; }
        double UnconfirmedBalanceDelta { get; }
        IEnumerable<IColoredBalance> ColoredBalances { get; }
    }

    public interface IAddressTransaction
    {
        string TransactionId { get; }
    }

    public interface IColoredBalance
    {
        string AssetId { get; }
        double Quantity { get; }
        double UnconfirmedQuantityDelta { get; }
    }

    public interface IAddressService
    {
        Task<IAddressBalance> GetBalanceAsync(string id);
    }
}
