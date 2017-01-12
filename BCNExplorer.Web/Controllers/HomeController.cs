using System.Threading.Tasks;
using System.Web.Mvc;
using BCNExplorer.Web.Models;
using Core.Block;
using Services.MainChain;

namespace BCNExplorer.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IBlockService _blockService;
        private readonly CachedMainChainRepository _cachedMainChainRepository;

        public HomeController(IBlockService blockService, CachedMainChainRepository cachedMainChainRepository)
        {
            _blockService = blockService;
            _cachedMainChainRepository = cachedMainChainRepository;
        }

        public async Task<ActionResult> Index()
        {
            var t = await _cachedMainChainRepository.GetMainChainAsync();
            var lastBlock = await _blockService.GetLastBlockHeaderAsync();

            return View(LastBlockViewModel.Create(lastBlock));
        }

        public string Version()
        {
            return App.Version;
        }
    }
}