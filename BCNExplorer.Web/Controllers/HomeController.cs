using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using BCNExplorer.Web.Models;
using Core.Block;

namespace BCNExplorer.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IBlockService _blockService;

        public HomeController(IBlockService blockService)
        {
            _blockService = blockService;
        }

        public async Task<ActionResult> Index()
        {
            var t = Request.GetOwinContext().Authentication.User.Identity.IsAuthenticated;
            var lastBlock = await _blockService.GetLastBlockHeaderAsync();

            return View(LastBlockViewModel.Create(lastBlock));
        }

        public string Version()
        {
            return App.Version;
        }
    }
}