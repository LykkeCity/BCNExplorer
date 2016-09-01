using System.Linq;
using System.Threading.Tasks;
using NBitcoin;
using NBitcoin.OpenAsset;
using Providers.BlockChainReader;
using Providers.Contracts.Ninja;
using Providers.TransportTypes;
using Providers.TransportTypes.Ninja;

namespace Providers.Providers.Ninja
{
    public class NinjaTransactionProvider
    {
        private readonly NinjaBlockChainReader _blockChainReader;

        public NinjaTransactionProvider(NinjaBlockChainReader blockChainReader)
        {
            _blockChainReader = blockChainReader;
        }

        public async Task<NinjaTransaction> GetAsync(string id)
        {
            var responce = await _blockChainReader.DoRequest<TransactionContract>($"transactions/{id}?colored=true");
            if (responce == null)
            {
                return null;
            }

            var transactionInfo = Transaction.Parse(responce.Hex);

            var inputs = responce.Inputs.ToList();
            var outputs = responce.Outputs.ToList();

            #region CalculateInputsWithReturnedChange

            foreach (var input in inputs.Where(inp => outputs.Any(x => x.Address == inp.Address)))
            {
                foreach (var output in outputs.Where(x => x.Address == input.Address && x.AssetId == input.AssetId).ToList())
                {
                    input.Value -= output.Value;
                    input.Quantity -= output.Quantity;

                    outputs.Remove(output);
                }
            }

            #endregion

            var result = new NinjaTransaction
            {
                TransactionId = responce.TransactionId,
                Hex = transactionInfo.ToHex(),
                IsCoinBase = transactionInfo.IsCoinBase,
                IsColor = transactionInfo.HasValidColoredMarker(),
                Block =  NinjaTransaction.BlockMinInfo.Create(responce.Block),
                Fees = responce.Fees,
                TransactionsByAssets = NinjaTransaction.InOutsByAsset.Create(inputs, outputs)
            };
            
            return result;
        }
    }
}
