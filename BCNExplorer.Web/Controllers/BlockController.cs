using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.SessionState;
using BCNExplorer.Web.Models;
using Core.Block;
using Providers.Providers.Ninja;

namespace BCNExplorer.Web.Controllers
{
    [SessionState(SessionStateBehavior.Disabled)]
    public class BlockController : Controller
    {
        private readonly IBlockService _blockService;

        public BlockController(IBlockService blockService)
        {
            _blockService = blockService;
        }

        [Route("block/{id}")]
        [OutputCache(Duration = 10 * 60, VaryByParam = "*")]
        public async Task<ActionResult> Index(string id)
        {
            int numBlockHeight;

            if(int.TryParse(id, out numBlockHeight))
            {
                var header = await _blockService.GetBlockHeaderAsync(numBlockHeight.ToString());
                if (header != null)
                {
                    return RedirectToAction("Index", "Block", new {id = header.Hash});
                }
            }

            var block = _blockService.GetBlockAsync(id);
            var lastBlock = _blockService.GetLastBlockHeaderAsync();

            await Task.WhenAll(block, lastBlock);

            if (block != null)
            {
                var result = BlockViewModel.Create(block.Result, lastBlock.Result);

                return View(result);
            }

            return View("NotFound");
        }
    }
}