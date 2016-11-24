using System;
using Newtonsoft.Json;

namespace Providers.Contracts.Ninja
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
        public int Height { get; set; }
        [JsonProperty("blockTime")]
        public DateTime Time { get; set; }
        [JsonProperty("confirmations")]
        public long Confirmations { get; set; }
        [JsonProperty("blockId")]
        public string BlockId { get; set; }
    }

    internal class BlockHeaderContract
    {
        [JsonProperty("additionalInformation")]
        public AdditionalInformationContract AdditionalInformation { get; set; }
    }

}
