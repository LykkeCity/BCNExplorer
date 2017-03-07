using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Core.AddressService;
using Core.Block;

namespace BCNExplorer.Web.Models
{
    public class AddressMainInfoViewModel
    {
        public string AddressId { get; set; }
        public string UncoloredAddress { get; set; }
        public string ColoredAddress { get; set; }

        public bool IsColoredAddress { get; set; }



        public static AddressMainInfoViewModel Create(IAddressMainInfo addressMainInfo)
        {
            return new AddressMainInfoViewModel
            {
                AddressId = addressMainInfo.AddressId,
                ColoredAddress = addressMainInfo.ColoredAddress,
                UncoloredAddress = addressMainInfo.UncoloredAddress,
                IsColoredAddress = addressMainInfo.IsColored
            };
        }
    }
}