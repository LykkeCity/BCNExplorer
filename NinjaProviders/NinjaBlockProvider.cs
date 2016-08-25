using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Settings;
using NBitcoin;
using NBitcoin.OpenAsset;
using Newtonsoft.Json;

namespace NinjaProviders
{
    public class NinjaBlock
    {
        public string Hash { get; set; }
        public long Height { get; set; }
        public DateTime Time { get; set; }
        public long Confirmations { get; set; }
        public double Difficulty { get; set; }
        public string MerkleRoot { get; set; }
        public long Nonce { get; set; }
        public int TotalTransactions { get; set; }
        public string PreviousBlock { get; set; }
    }

    internal class BlockNinjaResponce
    {
        [JsonProperty("additionalInformation")]
        public AdditionalInformationNinjaResponce AdditionalInformation { get; set; }

        [JsonProperty("block")]
        public string Hex { get; set; }
    }

    internal class AdditionalInformationNinjaResponce
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

    public class NinjaBlockProvider
    {
        private readonly string _ninjaBaseUrl;

        public NinjaBlockProvider(BaseSettings baseSettings)
        {
            _ninjaBaseUrl = baseSettings.NinjaUrl;
        }

        public async Task<NinjaBlock> GetAsync(string id)
        {
            var blockResponse = await NinjaBlockChainReader.DoRequest<BlockNinjaResponce>(_ninjaBaseUrl + $"blocks/{id}");
            if (blockResponse == null)
            {
                return null;
            }

            var block = Block.Parse(blockResponse.Hex);

            var result = new NinjaBlock
            {
                Confirmations = blockResponse.AdditionalInformation.Confirmations,
                Time = blockResponse.AdditionalInformation.Time,
                Height = blockResponse.AdditionalInformation.Height,
                Hash = block.Header.ToString(),
                TotalTransactions = block.Transactions.Count,
                Difficulty = block.Header.Bits.Difficulty,
                MerkleRoot = block.Header.HashMerkleRoot.ToString(),
                PreviousBlock = block.Header.HashPrevBlock.ToString(),
                Nonce = block.Header.Nonce
            };

            return result;
        } 
    }
}
