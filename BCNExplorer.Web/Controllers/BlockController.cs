using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.SessionState;
using BCNExplorer.Web.Models;
using Core.Block;

namespace BCNExplorer.Web.Controllers
{
    [SessionState(SessionStateBehavior.Disabled)]
    public class BlockController : Controller
    {
        private readonly IBlockService _blockService;
        private readonly ICachedBlockService _cachedBlockService;

        public BlockController(IBlockService blockService, ICachedBlockService cachedBlockService)
        {
            _blockService = blockService;
            _cachedBlockService = cachedBlockService;
        }

        [Route("block/{id}")]
        public async Task<ActionResult> Index(string id)
        {
            var block = _cachedBlockService.GetBlockAsync(id);
            var lastBlock = _blockService.GetLastBlockHeaderAsync();

            await Task.WhenAll(block, lastBlock);

            if (block.Result != null)
            {
                var result = BlockViewModel.Create(block.Result, lastBlock.Result);

                return View(result);
            }

            return View("NotFound");
        }
    }
}