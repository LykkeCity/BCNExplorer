using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Block;
using NBitcoin;
using NBitcoin.OpenAsset;
using Providers;
using Providers.Providers.Ninja;

namespace Services.BlockChain
{
    public class BlockHeader : IBlockHeader
    {
        public string Hash { get; set; }
        public int Height { get; set; }
        public DateTime Time { get; set; }
        public long Confirmations { get; set; }
        public bool IsFork => Height == -1;

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
        public IEnumerable<string> AllTransactionIds { get; set; }
        public IEnumerable<string> ColoredTransactionIds { get; set; }
        public IEnumerable<string> UncoloredTransactionIds { get; set; }

        public Block()
        {
            AllTransactionIds = Enumerable.Empty<string>();
            ColoredTransactionIds = Enumerable.Empty<string>();
            UncoloredTransactionIds = Enumerable.Empty<string>();
        }
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

        public Task<IBlock> GetBlockAsync(string id)
        {
            uint256 hash;
            int height;

            if (uint256.TryParse(id, out hash))
            {
                return GetBlockAsync(hash);
            }
            if (int.TryParse(id, out height))
            {
                return GetBlockAsync(height);
            }

            return null;
        }

        public async Task<IBlock> GetBlockAsync(uint256 hash)
        {
            var result = new Lazy<Block>(()=> new Block());

            var fillHeaderTask = _ninjaBlockProvider.GetHeaderAsync(hash.ToString())
                .ContinueWith(tsk =>
            {
                if (tsk.Result != null)
                {
                    FillHeaderData(tsk.Result, result.Value);
                }
            });

            var fillDbDataTask = Task.Run(() =>
            {
                var block =_indexerClientFactory.GetIndexerClient().GetBlock(hash);
                if (block != null)
                {
                    FillBlockDataFromDb(block, result.Value);
                }
            });

            await Task.WhenAll(fillHeaderTask, fillDbDataTask);
            
            return result.IsValueCreated? result.Value: null;
        }

        private async Task<IBlock> GetBlockAsync(int height)
        {
            var header = await _ninjaBlockProvider.GetHeaderAsync(height.ToString());
            if (header != null)
            {
                var result = new Block();
                var block = _indexerClientFactory.GetIndexerClient().GetBlock(uint256.Parse(header.Hash));

                FillHeaderData(header, result);
                FillBlockDataFromDb(block, result);

                return result;
            }

            return null;
        }

        private void FillHeaderData(NinjaBlockHeader header, Block result)
        {
            result.Confirmations = header.Confirmations;
            result.Height = header.Height;
        }

        private void FillBlockDataFromDb(NBitcoin.Block block, Block result)
        {
            result.Time = block.Header.BlockTime.DateTime;
            result.Hash = block.Header.GetHash().ToString();
            result.TotalTransactions = block.Transactions.Count;
            result.Difficulty = block.Header.Bits.Difficulty;
            result.MerkleRoot = block.Header.HashMerkleRoot.ToString();
            result.PreviousBlock = block.Header.HashPrevBlock.ToString();
            result.Nonce = block.Header.Nonce;
            result.AllTransactionIds = block.Transactions.Select(p => p.GetHash().ToString()).ToList();
            result.ColoredTransactionIds = block.Transactions.Where(p => p.HasValidColoredMarker()).Select(p => p.GetHash().ToString()).ToList();
            result.UncoloredTransactionIds = block.Transactions.Where(p => !p.HasValidColoredMarker()).Select(p => p.GetHash().ToString()).ToList();
        }
    }
}
