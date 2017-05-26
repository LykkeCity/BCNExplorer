using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BCNExplorer.Web.Models;
using Core.Asset;
using Core.Channel;
using Core.Transaction;

namespace BCNExplorer.Web.Controllers
{
    public class OffchainTransactionController: Controller
    {
        private readonly ICachedTransactionService _transactionService;
        private readonly IAssetService _assetService;
        private readonly IChannelRepository _channelRepository;

        
        public OffchainTransactionController(ICachedTransactionService transactionService,
            IAssetService assetService, 
            IChannelRepository channelRepository)
        {
            _transactionService = transactionService;
            _assetService = assetService;
            _channelRepository = channelRepository;
        }

        [Route("transaction/offchain/{id}")]
        public async Task<ActionResult> Index(string id)
        {
            var channel = _channelRepository.GetByOffchainTransactionId(id);
            var assetDictionary = _assetService.GetAssetDefinitionDictionaryAsync();

            await Task.WhenAll(channel, assetDictionary);

            if (channel.Result == null)
            {
                return View("NotFound");
            }

            var openTx = _transactionService.GetAsync(channel.Result.OpenTransactionId);
            var closeTx = _transactionService.GetAsync(channel.Result.CloseTransactionId);

            await Task.WhenAll(openTx, closeTx, assetDictionary);

            var openOnChainTx = TransactionViewModel.Create(openTx.Result, assetDictionary.Result);
            var closeOnChainTx = TransactionViewModel.Create(closeTx.Result, assetDictionary.Result);
            
            var offchainTransactions = OffChainTransactionViewModel.Create(channel.Result.OffchainTransactions,assetDictionary.Result).ToList();


            var channelViewModel = OffchainChannelViewModel.Create(openOnChainTx, closeOnChainTx, offchainTransactions);

            return View(OffchainTransactionDetailsViewModel.Create(channelViewModel, offchainTransactions.First(x => x.TransactionId ==id)));
        }
    }
}