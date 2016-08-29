using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NinjaProviders.TransportTypes;

namespace BCNExplorer.Web.Models
{
    public class AddressViewModel
    {
        public string AddressId { get; set; }
        public IEnumerable<string> TransactionIds { get; set; }
        public string UncoloredAddress { get; set; }
        public string ColoredAddress { get; set; }
        public double Balance { get; set; }

        public static AddressViewModel Create(NinjaAddress ninjaAddress)
        {
            return new AddressViewModel
            {
                AddressId = ninjaAddress.AddressId,
                TransactionIds = ninjaAddress.TransactionIds,
                UncoloredAddress = ninjaAddress.UncoloredAddress,
                ColoredAddress = ninjaAddress.ColoredAddress
            };
        }
    }
}