using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Core.AddressService;

namespace BCNExplorer.Web.Models
{
    public class AddressMainInfoViewModel
    {
        public string AddressId { get; set; }
        public string UncoloredAddress { get; set; }
        public string ColoredAddress { get; set; }

        public static AddressMainInfoViewModel Create(IAddressMainInfo addressMainInfo)
        {
            return new AddressMainInfoViewModel
            {
                AddressId = addressMainInfo.AddressId,
                ColoredAddress = addressMainInfo.ColoredAddress,
                UncoloredAddress = addressMainInfo.UncoloredAddress
            };
        }
    }
}