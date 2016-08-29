using Newtonsoft.Json;

namespace NinjaProviders.Contracts
{
    public class WhatIsItAdrressContract
    {
        [JsonProperty("coloredAddress")]
        public string ColoredAddress { get; set; }
        [JsonProperty("uncoloredAddress")]
        public string UncoloredAddress { get; set; }
    }

    public class AddressTransactionListContract
    {
        [JsonProperty("operations")]
        public AddressTransactionListItemContract[] Transactions { get; set; }
    }

    public class AddressTransactionListItemContract
    {
        [JsonProperty("transactionId")]
        public string TxId { get; set; }
    }
}
