using System.Linq;
using System.Threading.Tasks;
using NBitcoin;
using NBitcoin.OpenAsset;
using Providers.BlockChainReader;
using Providers.Contracts.Ninja;
using Providers.Helpers;
using Providers.TransportTypes.Ninja;

namespace Providers.Providers.Ninja
{
    public class TransactionProvider
    {
        private readonly NinjaBlockChainReader _blockChainReader;

        public TransactionProvider(NinjaBlockChainReader blockChainReader)
        {
            _blockChainReader = blockChainReader;
        }

        public async Task<NinjaTransaction> GetAsync(string id, bool calculateInputsWithReturnedChange = true)
        {
            var responce = await _blockChainReader.GetAsync<TransactionContract>($"transactions/{id}?colored=true");
            if (responce == null)
            {
                return null;
            }

            var transactionInfo = Transaction.Parse(responce.Hex);
            
            var inputs = responce.Inputs.ToList();
            var outputs = responce.Outputs.ToList();

            #region CalculateInputsWithReturnedChange

            if (calculateInputsWithReturnedChange)
            {
                foreach (var input in inputs.Where(inp => outputs.Any(x => x.Address == inp.Address)))
                {
                    foreach (var output in outputs.Where(x => x.Address == input.Address && x.AssetId == input.AssetId).ToList())
                    {
                        input.Value -= output.Value;
                        input.Quantity -= output.Quantity;

                        outputs.Remove(output);
                    }
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
