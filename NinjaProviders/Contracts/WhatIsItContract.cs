using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NinjaProviders.Contracts
{
    public class WhatIsItContract
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        public const string UncoloredAddressType = "PUBKEY_ADDRESS";
        public const string ColoredAddressType = "COLORED_ADDRESS";
    }
}
