using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BCNExplorer.Web.Models
{
    public class AddressViewModel
    {
        public string AddressId { get; set; }

        public IEnumerable<string> TransactionIds { get; set; }
        public string UncoloredAddress { get; set; }
        public double Balance { get; set; }
    }
}