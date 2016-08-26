using System;
using System.Linq;
using System.Threading.Tasks;
using NBitcoin;
using NBitcoin.OpenAsset;
using Newtonsoft.Json;
using NinjaProviders.BlockChainReader;
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
                Hash = blockResponse.AdditionalInformation.BlockId,
                TotalTransactions = block.Transactions.Count,
                Difficulty = block.Header.Bits.Difficulty,
                MerkleRoot = block.Header.HashMerkleRoot.ToString(),
                PreviousBlock = block.Header.HashPrevBlock.ToString(),
                Nonce = block.Header.Nonce
            };

            return result;
        }

        public async Task<NinjaBlockHeader> GetHeaderAsync(string id)
        {
            var blockResponse = await _blockChainReader.DoRequest<BlockContractHeader>($"blocks/{id}?headeronly=true");
            if (blockResponse == null)
            {
                return null;
            }

            var result = new NinjaBlockHeader
            {
                Confirmations = blockResponse.AdditionalInformation.Confirmations,
                Time = blockResponse.AdditionalInformation.Time,
                Height = blockResponse.AdditionalInformation.Height,
                Hash = blockResponse.AdditionalInformation.BlockId,
            };

            return result;
        }
    }
}
