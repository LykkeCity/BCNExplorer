using System.Linq;
using System.Threading.Tasks;
using NBitcoin;
using Providers.BlockChainReader;
using Providers.Contracts.Ninja;
using Providers.TransportTypes;
using Providers.TransportTypes.Ninja;

namespace Providers.Providers.Ninja
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
                Nonce = block.Header.Nonce,
                TransactionIds = block.Transactions.Select(p=>p.GetHash().ToString())
            };

            return result;
        }

        public async Task<NinjaBlockHeader> GetHeaderAsync(string id)
        {
            var blockResponse = await _blockChainReader.DoRequest<BlockHeaderContract>($"blocks/{id}?headeronly=true");
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
            var blockResponse = await _blockChainReader.DoRequest<BlockHeaderContract>($"blocks/tip?headeronly=true");
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
