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
        public async Task<ActionResult> Index(string id)
        {
            var mainInfo = _addressProvider.GetMainInfoAsync(id);
            var isOffchainHub = _channelService.IsHubAsync(id); 

            await Task.WhenAll(mainInfo, isOffchainHub);

            if (mainInfo.Result != null)
            {
                return View(AddressMainInfoViewModel.Create(mainInfo.Result, isOffchainHub.Result));
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
            var balance = _addressProvider.GetBalanceAsync(id, at);
            var assetDefinitionDictionary = _assetService.GetAssetDefinitionDictionaryAsync();
            var lastBlock = _blockService.GetLastBlockHeaderAsync();
            var offchainChannels = _channelService.GetByAddressAsync(id, ChannelStatusQueryType.OpenOnly);
            Task<IBlockHeader> atBlockTask;

            if (at != null)
            {
                atBlockTask = _blockService.GetBlockHeaderAsync(at.ToString());
            }
            else
            {
                atBlockTask = Task.FromResult<IBlockHeader>(null);
            }

            await Task.WhenAll(balance, assetDefinitionDictionary, lastBlock, atBlockTask, offchainChannels);

            if (balance.Result != null)
            {
                return View("Balance" ,AddressBalanceViewModel.Create(balance.Result,
                    assetDefinitionDictionary.Result,
                    lastBlock.Result,
                    atBlockTask.Result,
                    offchainChannels.Result));
            }

            if (at != null && atBlockTask.Result == null)
            {
                return RedirectToAction("BalanceAtBlock", new {id = id, at = lastBlock.Result});
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
            var channels = _channelService.GetByAddressAsync(id);

            await Task.WhenAll(onchainTransactions, assetDictionary, channels);

            return View(AddressTransactionsViewModel.Create(onchainTransactions.Result, channels.Result, assetDictionary.Result));
        } 
    }
}