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
        public string UncoloredAddress { get; set; }
        public string ColoredAddress { get; set; }
        public double Balance { get; set; }
        public int TotalTransactions { get; set; }
        public TransactionIdList TransactionIdList { get; set; }
        private const int PageSize = 20;

        public static AddressViewModel Create(NinjaAddress ninjaAddress)
        {
            return new AddressViewModel
            {
                AddressId = ninjaAddress.AddressId,
                TransactionIdList = new TransactionIdList(ninjaAddress.TransactionIds, PageSize),
                UncoloredAddress = ninjaAddress.UncoloredAddress,
                ColoredAddress = ninjaAddress.ColoredAddress,
                TotalTransactions = ninjaAddress.TransactionIds.Count()
            };
        }
    }
}