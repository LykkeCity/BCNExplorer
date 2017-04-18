using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Providers.Providers.Ninja;

namespace Providers.Contracts.BtcBalance
{
    public class BalanceViewModelContract
    {
        [JsonProperty("error")]
        public BalanceErrorContract Error { get; set; }

        [JsonProperty("balance")]
        public NinjaAddressSummary Data { get; set; }
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
