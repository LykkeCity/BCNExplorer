using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.AddressService
{
    public interface ICachedAddressService
    {
        Task<IAddressTransactions> GetTransactions(string id);
    }
}
