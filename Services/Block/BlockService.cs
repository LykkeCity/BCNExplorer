using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Block;
using NBitcoin;
using Providers;
using Providers.Providers.Ninja;
using Providers.TransportTypes.Ninja;

namespace Services.BlockChain
{
    public class BlockHeader : IBlockHeader
    {
        public string Hash { get; set; }
        public int Height { get; set; }
        public DateTime Time { get; set; }
        public long Confirmations { get; set; }

        public static BlockHeader Create(string hash, int height, DateTime time, long confirmations)
        {
            return new BlockHeader
            {
                Confirmations = confirmations,
                Hash = hash,
                Height = height,
                Time = time
            };
        }
    }

    public class Block:IBlock
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

    public class BlockService:IBlockService
    {
        private readonly NinjaBlockProvider _ninjaBlockProvider;
        private readonly IndexerClientFactory _indexerClientFactory;

        public BlockService(NinjaBlockProvider ninjaBlockProvider, 
            IndexerClientFactory indexerClientFactory)
        {
            _ninjaBlockProvider = ninjaBlockProvider;
            _indexerClientFactory = indexerClientFactory;
        }

        public async Task<IBlockHeader> GetBlockHeaderAsync(string id)
        {
            var ninja = await _ninjaBlockProvider.GetHeaderAsync(id);
            if (ninja != null)
            {
                return BlockHeader.Create(ninja.Hash, ninja.Height, ninja.Time, ninja.Confirmations);
            }

            return null;
        }

        public async Task<IBlockHeader> GetLastBlockHeaderAsync()
        {
            var ninja = await _ninjaBlockProvider.GetLastBlockAsync();
            if (ninja != null)
            {
                return BlockHeader.Create(ninja.Hash, ninja.Height, ninja.Time, ninja.Confirmations);
            }

            return null;
        }

        public async Task<IBlock> GetBlockAsync(string id)
        {
            var header = await _ninjaBlockProvider.GetHeaderAsync(id);
            if (header != null)
            {
                var result = new Block
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
    }
}
