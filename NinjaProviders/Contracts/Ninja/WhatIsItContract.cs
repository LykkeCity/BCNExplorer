using Newtonsoft.Json;

namespace Providers.Contracts.Ninja
{
    public class WhatIsItContract
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        public const string UncoloredAddressType = "PUBKEY_ADDRESS";
        public const string ColoredAddressType = "COLORED_ADDRESS";
    }
}
