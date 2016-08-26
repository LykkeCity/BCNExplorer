using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NinjaProviders.Contracts
{
    internal class BlockContract
    {
        [JsonProperty("additionalInformation")]
        public AdditionalInformationContract AdditionalInformation { get; set; }

        [JsonProperty("block")]
        public string Hex { get; set; }
    }

    internal class AdditionalInformationContract
    {
        [JsonProperty("height")]
        public long Height { get; set; }
        [JsonProperty("blockTime")]
        public DateTime Time { get; set; }
        [JsonProperty("confirmations")]
        public long Confirmations { get; set; }
        [JsonProperty("blockId")]
        public string BlockId { get; set; }
    }

    internal class BlockContractHeader
    {
        [JsonProperty("additionalInformation")]
        public AdditionalInformationContract AdditionalInformation { get; set; }
    }

}
