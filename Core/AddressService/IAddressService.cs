using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.AddressService
{
    public interface IAddressBalance
    {
        string AddressId { get; set; }
        IEnumerable<string> TransactionIds { get; set; }
        double TotalTransactions { get; set; }
        string UncoloredAddress { get; set; }
        string ColoredAddress { get; set; }
        double Balance { get; set; }
        double UnconfirmedBalanceDelta { get; set; }
        IEnumerable<IColoredBalance> ColoredBalances { get; set; }
    }

    public interface IColoredBalance
    {
        string AssetId { get; set; }
        double Quantity { get; set; }
        double UnconfirmedQuantityDelta { get; set; }
    }

    public interface IAddressService
    {
        Task<IAddressBalance> GetBalanceAsync(string id);
    }
}
