using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.AssetBlockChanges
{
    public interface IAddress
    {
        string LegacyAddress { get; }
        string ColoredAddress { get;  }
    }

    public class Address: IAddress
    {
        public string LegacyAddress { get; set; }
        public string ColoredAddress { get; set; }
    }

    public interface IAddressRepository
    {
        Task AddAsync(params IAddress[] addresses);
    }
}
