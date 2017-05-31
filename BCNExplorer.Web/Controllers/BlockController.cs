using System.Linq;
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

        private const int OffchainChannelsPageSize = 1;

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
            var offchainChannelsCount = _channelService.GetCountByBlockAsync(id);

            await Task.WhenAll(block, lastBlock, offchainChannelsCount);

            if (block.Result != null)
            {
                var result = BlockViewModel.Create(block.Result, lastBlock.Result, offchainChannelsCount.Result, OffchainChannelsPageSize);

                return View(result);
            }

            return View("NotFound");
        }

        [Route("block/offchainchannelpage")]
        public async Task<ActionResult> OffchainChannelPage(string block, int page)
        {
            var channels = _channelService.GetByBlockAsync(block,
                channelStatusQueryType: ChannelStatusQueryType.All,
                pageOptions: PageOptions.Create(page, OffchainChannelsPageSize));
            var assetDictionary = _assetService.GetAssetDefinitionDictionaryAsync();

            await Task.WhenAll(channels, assetDictionary);

            return View(channels.Result.Select(p => OffchainFilledChannelViewModel.Create(p, assetDictionary.Result)));
        }
    }
}