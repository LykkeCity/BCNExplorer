using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.SessionState;
using BCNExplorer.Web.Models;
using Core.Asset;
using Core.Block;
using Core.Channel;

namespace BCNExplorer.Web.Controllers
{
    [SessionState(SessionStateBehavior.Disabled)]
    public class BlockController : Controller
    {
        private readonly IBlockService _blockService;
        private readonly ICachedBlockService _cachedBlockService;
        private readonly IChannelService _channelService;
        private readonly IAssetService _assetService;

        public BlockController(IBlockService blockService, 
            ICachedBlockService cachedBlockService,
            IChannelService channelService, 
            IAssetService assetService)
        {
            _blockService = blockService;
            _cachedBlockService = cachedBlockService;
            _channelService = channelService;
            _assetService = assetService;
        }

        [Route("block/{id}")]
        public async Task<ActionResult> Index(string id)
        {
            var block = _cachedBlockService.GetBlockAsync(id);
            var lastBlock = _blockService.GetLastBlockHeaderAsync();
            var offchainChannels = _channelService.GetByBlockAsync(id);
            var assetDictionary = _assetService.GetAssetDefinitionDictionaryAsync();

            await Task.WhenAll(block, lastBlock, offchainChannels, assetDictionary);

            if (block.Result != null)
            {
                var result = BlockViewModel.Create(block.Result, lastBlock.Result, offchainChannels.Result, assetDictionary.Result);

                return View(result);
            }

            return View("NotFound");
        }
    }
}