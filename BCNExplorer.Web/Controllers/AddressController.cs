using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.SessionState;
using BCNExplorer.Web.Models;
using Core.AddressService;
using Core.Asset;
using Core.Block;
using Core.Channel;
using Providers.Helpers;
using Services.MainChain;

namespace BCNExplorer.Web.Controllers
{
    [SessionState(SessionStateBehavior.Disabled)]
    public class AddressController:Controller
    {
        private readonly IAddressService _addressProvider;
        private readonly IAssetService _assetService;
        private readonly IBlockService _blockService;
        private readonly IChannelService _channelService;
        private readonly ICachedAddressService _cachedAddressService;
        private readonly CachedMainChainService _mainChainService;

        public AddressController(IAddressService addressProvider, 
            IAssetService assetService, 
            IBlockService blockService, 
            CachedMainChainService mainChainService, 
            ICachedAddressService cachedAddressService, 
            IChannelService channelService)
        {
            _addressProvider = addressProvider;
            _assetService = assetService;
            _blockService = blockService;
            _mainChainService = mainChainService;
            _cachedAddressService = cachedAddressService;
            _channelService = channelService;
        }
        
        [Route("address/{id}")]
        public async Task<ActionResult> Index(string id, int? at)
        {
            var mainInfoTask = _addressProvider.GetMainInfoAsync(id);

            await Task.WhenAll(mainInfoTask);

            if (mainInfoTask.Result != null)
            {
                return View(AddressMainInfoViewModel.Create(mainInfoTask.Result));
            }

            return View("NotFound");
        }

        [Route("address/balance/{id}")]
        public Task<ActionResult> Balance(string id)
        {
            return BalanceAtBlockInner(id, null);
        }

        [OutputCache(Duration = 60 * 60, VaryByParam = "*")]
        public Task<ActionResult> BalanceAtBlock(string id, int? at)
        {
            return BalanceAtBlockInner(id, at);
        }

        private async Task<ActionResult> BalanceAtBlockInner(string id, int? at)
        {
            var balanceTask = _addressProvider.GetBalanceAsync(id, at);
            var assetDefinitionDictionaryTask = _assetService.GetAssetDefinitionDictionaryAsync();
            var lastBlockTask = _blockService.GetLastBlockHeaderAsync();
            Task<IBlockHeader> atBlockTask;

            if (at != null)
            {
                atBlockTask = _blockService.GetBlockHeaderAsync(at.ToString());
            }
            else
            {
                atBlockTask = Task.FromResult<IBlockHeader>(null);
            }

            await Task.WhenAll(balanceTask, assetDefinitionDictionaryTask, lastBlockTask, atBlockTask);

            if (balanceTask.Result != null)
            {
                return View("Balance" ,AddressBalanceViewModel.Create(balanceTask.Result,
                    assetDefinitionDictionaryTask.Result,
                    lastBlockTask.Result,
                    atBlockTask.Result));
            }

            if (at != null && atBlockTask.Result == null)
            {
                return RedirectToAction("BalanceAtBlock", new {id = id, at = lastBlockTask.Result});
            }

            return new HttpNotFoundResult();
        }

        [Route("address/balance/{id}/time/")]
        public async Task<ActionResult> BalanceAtTime(DateTime at, string id)
        {
            var mainChain = await _mainChainService.GetMainChainAsync();

            var block = mainChain.GetClosestToTimeBlock(at);

            return RedirectToAction("BalanceAtBlock", new { id = id, at = block?.Height ?? 0 });
        }
        
        [Route("address/transactions/{id}")]
        public async Task<ActionResult> Transactions(string id)
        {
            var onchainTransactions = _cachedAddressService.GetTransactions(id);
            var assetDictionary = _assetService.GetAssetDefinitionDictionaryAsync();
            var channels = _channelService.GetByAddress(id);

            await Task.WhenAll(onchainTransactions, assetDictionary, channels);

            return View(AddressTransactionsViewModel.Create(onchainTransactions.Result, channels.Result, assetDictionary.Result));
        } 
    }
}