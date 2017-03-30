using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Providers.Contracts.BtcBalance
{
    public class BalanceViewModelContract
    {
        [JsonProperty("error")]
        public BalanceErrorContract Error { get; set; }

        [JsonProperty("balance")]
        public NinjaBalanceSummaryContract Data { get; set; }
    }

    public class NinjaBalanceSummaryContract
    {
        [JsonProperty("confirmed")]
        public InnerDataContract Confirmed { get; set; }

        [JsonProperty("unConfirmed")]
        public InnerDataContract Unconfirmed { get; set; }

        [JsonProperty("spendable")]
        public InnerDataContract Spendable { get; set; }

        [JsonProperty("immature")]
        public InnerDataContract Immature { get; set; }

        public class InnerDataContract
        {
            [JsonProperty("transactionCount")]
            public int TotalTransactions { get; set; }

            [JsonProperty("amount")]
            public long Balance { get; set; }

            [JsonProperty("assets")]
            public List<AddressAssetContract> Assets { get; set; }
        }
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

    public class BalanceErrorContract
    {
        [JsonProperty("statusCode")]
        public int StatusCode { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }
    }
}
