using System.Threading.Tasks;
using System.Web.Mvc;
using BCNExplorer.Web.Models;
using Providers.Providers.Ninja;

namespace BCNExplorer.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly BlockProvider _blockProvider;

        public HomeController(BlockProvider blockProvider)
        {
            _blockProvider = blockProvider;
        }

        public async Task<ActionResult> Index()
        {
            var lastBlock = await _blockProvider.GetLastBlockAsync();

            return View(LastBlockViewModel.Create(lastBlock));
        }

        public string Version()
        {
            return Product.Version;
        }
    }
}