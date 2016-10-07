using System.Collections.Generic;

namespace Providers.TransportTypes.Ninja
{
    public class NinjaAddress
    {
        public string AddressId { get; set; }
        public IEnumerable<string> TransactionIds { get; set; }
        public double TotalTransactions { get; set; } 
        public string UncoloredAddress { get; set; }
        public string ColoredAddress { get; set; }
        public double Balance { get; set; }
        public double UnconfirmedBalanceDelta { get; set; }
        public IEnumerable<Asset> Assets { get; set; } 

        public class Asset
        {
            public string AssetId { get; set; }
            public double Quantity { get; set; }
        }
    }
}
