using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.AddressService
{
    public interface IAddressBalance
    {
        string AddressId { get; }
        int TotalTransactions { get; }
        double Balance { get; }
        double UnconfirmedBalanceDelta { get; }
        IEnumerable<IColoredBalance> ColoredBalances { get; }
    }

    public interface IAddressMainInfo
    {
        string AddressId { get; }
        int TotalTransactions { get; }
        string UncoloredAddress { get; }
        string ColoredAddress { get; }
    }

    public interface IAddressTransactions
    {
        IEnumerable<IAddressTransaction> All { get; }
        IEnumerable<IAddressTransaction> Send { get; }
        IEnumerable<IAddressTransaction> Received { get; }
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
        Task<IAddressBalance> GetBalanceAsync(string id, int? at = null);

        Task<IAddressMainInfo> GetMainInfoAsync(string id);

        Task<IAddressTransactions> GetTransactions(string id);
    }
}
