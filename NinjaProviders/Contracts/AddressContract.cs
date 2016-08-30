using Newtonsoft.Json;

namespace NinjaProviders.Contracts
{

    #region WhatIsItAdrressContract

    public class WhatIsItAdrressContract
    {
        [JsonProperty("coloredAddress")]
        public string ColoredAddress { get; set; }
        [JsonProperty("uncoloredAddress")]
        public string UncoloredAddress { get; set; }
    }
    
    #endregion


    #region AddressTransactionListContract

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

    #endregion

    #region AddressSummaryContract

    internal class AddressSummaryContract
    {
        [JsonProperty("confirmed")]
        public AddressSummaryInnerContract Confirmed { get; set; }
    }

    public class AddressSummaryInnerContract
    {
        [JsonProperty("transactionCount")]
        public int TotalTransactions { get; set; }
        [JsonProperty("amount")]
        public long Balance { get; set; }
        [JsonProperty("assets")]
        public AddressAssetContract[] Assets { get; set; }
    }

    public class AddressAssetContract
    {
        [JsonProperty("asset")]
        public string AssetId { get; set; }
        [JsonProperty("quantity")]
        public long Quantity { get; set; }
        [JsonProperty("received")]
        public long Received { get; set; }
    }

    #endregion
}
