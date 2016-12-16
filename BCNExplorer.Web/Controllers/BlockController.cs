using System.Threading.Tasks;
using System.Web.Mvc;
using BCNExplorer.Web.Models;
using Core.Block;
using Providers.Providers.Ninja;

namespace BCNExplorer.Web.Controllers
{
    public class BlockController : Controller
    {
        private readonly IBlockService _blockService;

        public BlockController(IBlockService blockService)
        {
            _blockService = blockService;
        }

        [Route("block/{id}")]
        //[OutputCache(Duration = 10 * 60, VaryByParam = "*")]
        public async Task<ActionResult> Index(string id)
        {
            var block = await _blockService.GetBlockAsync(id);

            if (block != null)
            {
                var result = BlockViewModel.Create(block);

                return View(result);
            }

            return View("NotFound");
        }
    }
}