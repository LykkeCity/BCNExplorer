using System;
using System.Linq;
using System.Threading.Tasks;
using NBitcoin;
using Providers.BlockChainReader;
using Providers.Contracts.Ninja;
using Providers.TransportTypes.Ninja;

namespace Providers.Providers.Ninja
{
    public class BlockProvider
    {
        private readonly NinjaBlockChainReader _blockChainReader;
        private readonly IndexerClientFactory _indexerClientFactory;

        public BlockProvider(NinjaBlockChainReader blockChainReader, IndexerClientFactory indexerClientFactory)
        {
            _blockChainReader = blockChainReader;
            _indexerClientFactory = indexerClientFactory;
        }

        public async Task<BlockDTO> GetAsync(string id)
        {
            var header = await GetHeaderAsync(id);
            if (header != null)
            {
                var result = new BlockDTO
                {
                    Confirmations = header.Confirmations,
                    Height = header.Height,
                    Time = header.Time,
                    Hash = header.Hash
                };

                var block = _indexerClientFactory.GetIndexerClient().GetBlock(uint256.Parse(result.Hash));

                result.TotalTransactions = block.Transactions.Count;
                result.Difficulty = block.Header.Bits.Difficulty;
                result.MerkleRoot = block.Header.HashMerkleRoot.ToString();
                result.PreviousBlock = block.Header.HashPrevBlock.ToString();
                result.Nonce = block.Header.Nonce;
                result.TransactionIds = block.Transactions.Select(p => p.GetHash().ToString());
          
                return result;
            }

            return null;
        }

        public async Task<NinjaBlockHeader> GetHeaderAsync(string id)
        {
            var blockResponse = await _blockChainReader.GetAsync<BlockHeaderContract>($"blocks/{id}?headeronly=true");
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

        public async Task<NinjaBlockHeader> GetLastBlockAsync()
        {
            var blockResponse = await _blockChainReader.GetAsync<BlockHeaderContract>($"blocks/tip?headeronly=true");
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
