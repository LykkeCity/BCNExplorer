using System;
using Newtonsoft.Json;

namespace NinjaProviders.Contracts
{
    internal class TransactionContract
    {
        public string TransactionId { get; set; }

        [JsonProperty("block")]
        public BlockModel Block { get; set; }

        [JsonProperty("receivedCoins")]
        public BitCoinInOut[] ReceivedCoins { get; set; }

        [JsonProperty("spentCoins")]
        public BitCoinInOut[] SpentCoins { get; set; }

        [JsonProperty("firstSeen")]
        public DateTime FirstSeen { get; set; }

        #region Classes
        public class BlockModel
        {
            [JsonProperty("confirmations")]
            public int Confirmations { get; set; }

            [JsonProperty("height")]
            public int Height { get; set; }

            [JsonProperty("blockId")]
            public string BlockId { get; set; }

            [JsonProperty("blockTime")]
            public DateTime BlockTime { get; set; }
        }

        public class BitCoinInOut
        {
            [JsonProperty("address")]
            public string Address { get; set; }

            [JsonProperty("transactionId")]
            public string TransactionId { get; set; }

            [JsonProperty("index")]
            public int Index { get; set; }

            [JsonProperty("value")]
            public double Value { get; set; }

            [JsonProperty("scriptPubKey")]
            public string ScriptPubKey { get; set; }

            [JsonProperty("assetId")]
            public string AssetId { get; set; }

            [JsonProperty("quantity")]
            public double Quantity { get; set; }
        }

        #endregion
    }
}
