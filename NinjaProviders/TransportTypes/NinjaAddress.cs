using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjaProviders.TransportTypes
{
    public class NinjaAddress
    {
        public string AddressId { get; set; }
        public IEnumerable<string> TransactionIds { get; set; }
        public string UncoloredAddress { get; set; }
        public string ColoredAddress { get; set; }
        public double Balance { get; set; }
    }
}
