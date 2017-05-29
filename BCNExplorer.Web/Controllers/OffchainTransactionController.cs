using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.SessionState;
using BCNExplorer.Web.Models;
using Core.Asset;
using Core.Channel;
using Core.Transaction;

namespace BCNExplorer.Web.Controllers
{
    [SessionState(SessionStateBehavior.Disabled)]
    public class OffchainTransactionController: Controller
    {
        private readonly IAssetService _assetService;
        private readonly IChannelService _channelService;

        
        public OffchainTransactionController(ICachedTransactionService transactionService,
            IAssetService assetService,
            IChannelService channelService)
        {
            _assetService = assetService;
            _channelService = channelService;
        }

        [Route("transaction/offchain/{id}")]
        public async Task<ActionResult> Index(string id)
        {
            var channel = _channelService.GetByOffchainTransactionIdAsync(id);
            var assetDictionary = _assetService.GetAssetDefinitionDictionaryAsync();

            await Task.WhenAll(channel, assetDictionary);

            if (channel.Result == null)
            {
                return View("NotFound");
            }

            var channelViewModel = OffchainChannelViewModel.Create(channel.Result, assetDictionary.Result);

            return View(OffchainTransactionDetailsViewModel.Create(channelViewModel, id));
        }
    }
}