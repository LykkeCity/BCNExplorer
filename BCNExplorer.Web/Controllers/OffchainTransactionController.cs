using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BCNExplorer.Web.Models;
using Core.AddressService;
using Core.Asset;
using Core.Transaction;

namespace BCNExplorer.Web.Controllers
{
    public class OffchainTransactionController: Controller
    {
        private readonly ITransactionService _transactionService;
        private readonly IAssetService _assetService;
        
        public OffchainTransactionController(ITransactionService transactionService, IAssetService assetService)
        {
            _transactionService = transactionService;
            _assetService = assetService;
        }

        [Route("transaction/offchain/{id}")]
        public async Task<ActionResult> Index(string id)
        {
            var openTransactionId = "5ca252fa8a20d5c955f45999f9e9c358057cbb410f24c227b2a3ba1aa0b815f9";
            var closeTransactionId = "ebcc4dc1d376a0ef652bdefbb9e1381378989f9cc7fe2530d5a580353a930b9a";

            var assetDictionary = _assetService.GetAssetDefinitionDictionaryAsync();
            var openTx = _transactionService.GetAsync(openTransactionId);
            var closeTx = _transactionService.GetAsync(closeTransactionId);

            await Task.WhenAll(openTx, closeTx, assetDictionary);

            var openOnChainTx = TransactionViewModel.Create(openTx.Result, assetDictionary.Result);
            var closeOnChainTx = TransactionViewModel.Create(closeTx.Result, assetDictionary.Result);

            var asset = AssetViewModel.CreateNotFoundAsset("Adas");
            var offchainTransactions = new[] {
                OffChainTransactionViewModel.Create(
                    transactionId: "transactionId",
                    asset:AssetViewModel.BtcAsset.Value, 
                    dateTime:DateTime.UtcNow,
                    isRevoked:false,
                    hubAddress:"hubAddress",
                    address1: "address1",
                    address2: "address2",
                    address1Quantity: 100,
                    address2Quantity: 200,
                    address1QuantityDiff: 150,
                    address2QuantityDiff: 250),
                OffChainTransactionViewModel.Create(
                    transactionId: id,
                    asset:asset,
                    dateTime:DateTime.UtcNow,
                    isRevoked:false,
                    hubAddress:"hubAddress",
                    address1: "address1",
                    address2: "address2",
                    address1Quantity: 100,
                    address2Quantity: 200,
                    address1QuantityDiff: 150,
                    address2QuantityDiff: 250),

            };


            var channel = OffchainChannelViewModel.Create(openOnChainTx, closeOnChainTx, offchainTransactions);

            return View(OffchainTransactionDetailsViewModel.Create(channel, offchainTransactions.First(x => x.TransactionId ==id)));
        }
    }
}