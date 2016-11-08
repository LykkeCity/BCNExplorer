using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.AssetBlockChanges
{
    public interface IAddress
    {
        string ColoredAddress { get; }
    }

    public class Address: IAddress
    {
        public string ColoredAddress { get; set; }
    }

    public interface IAddressRepository
    {
        Task AddAsync(params IAddress[] addresses);
        Task<IEnumerable<IAddress>> GetAllAsync();
    }
}
