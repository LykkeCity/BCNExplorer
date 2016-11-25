using System.Threading.Tasks;
using System.Web.Mvc;
using BCNExplorer.Web.Models;
using Providers.Providers.Ninja;

namespace BCNExplorer.Web.Controllers
{
    public class BlockController : Controller
    {
        private readonly BlockProvider _blockProvider;

        public BlockController(BlockProvider blockProvider)
        {
            _blockProvider = blockProvider;
        }

        [Route("block/{id}")]
        [OutputCache(Duration = 10 * 60, VaryByParam = "*")]
        public async Task<ActionResult> Index(string id)
        {
            var ninjaBlock = await _blockProvider.GetAsync(id);

            if (ninjaBlock != null)
            {
                var result = BlockViewModel.Create(ninjaBlock);

                return View(result);
            }

            return View("NotFound");
        }
    }
}