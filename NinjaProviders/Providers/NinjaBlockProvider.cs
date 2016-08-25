using System;
using System.Threading.Tasks;
using NBitcoin;
using Newtonsoft.Json;
using NinjaProviders.BlockChainReder;
using NinjaProviders.Contracts;
using NinjaProviders.TransportTypes;

namespace NinjaProviders.Providers
{
    public class NinjaBlockProvider
    {
        private readonly NinjaBlockChainReader _blockChainReader;

        public NinjaBlockProvider(NinjaBlockChainReader blockChainReader)
        {
            _blockChainReader = blockChainReader;
        }

        public async Task<NinjaBlock> GetAsync(string id)
        {
            var blockResponse = await _blockChainReader.DoRequest<BlockContract>($"blocks/{id}");
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
