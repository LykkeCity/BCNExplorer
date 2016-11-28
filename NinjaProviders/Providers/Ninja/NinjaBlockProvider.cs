using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NBitcoin;
using Providers.BlockChainReader;
using Providers.Contracts.Ninja;
using Providers.TransportTypes.Ninja;

namespace Providers.Providers.Ninja
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
        public IEnumerable<string> TransactionIds { get; set; }
    }

    public class NinjaBlockHeader
    {
        public string Hash { get; set; }
        public int Height { get; set; }
        public DateTime Time { get; set; }
        public long Confirmations { get; set; }
    }

    public class NinjaBlockProvider
    {
        private readonly NinjaBlockChainReader _blockChainReader;
        private readonly IndexerClientFactory _indexerClientFactory;

        public NinjaBlockProvider(NinjaBlockChainReader blockChainReader, IndexerClientFactory indexerClientFactory)
        {
            _blockChainReader = blockChainReader;
            _indexerClientFactory = indexerClientFactory;
        }

        public async Task<NinjaBlock> GetAsync(string id)
        {
            var header = await GetHeaderAsync(id);
            if (header != null)
            {
                var result = new NinjaBlock
                {
                    Confirmations = header.Confirmations,
                    Height = header.Height
                };

                var block = _indexerClientFactory.GetIndexerClient().GetBlock(uint256.Parse(header.Hash));

                result.Time = block.Header.BlockTime.DateTime;
                result.Hash = block.Header.GetHash().ToString();
                result.TotalTransactions = block.Transactions.Count;
                result.Difficulty = block.Header.Bits.Difficulty;
                result.MerkleRoot = block.Header.HashMerkleRoot.ToString();
                result.PreviousBlock = block.Header.HashPrevBlock.ToString();
                result.Nonce = block.Header.Nonce;
                result.TransactionIds = block.Transactions.Select(p => p.GetHash().ToString()).ToList();
          
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
