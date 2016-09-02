using System.Threading.Tasks;
using System.Web.Mvc;
using BCNExplorer.Web.Models;
using Providers.Providers.Asset;

namespace BCNExplorer.Web.Controllers
{
    public class AssetController : Controller
    {
        private readonly AssetProvider _assetProvider;

        public AssetController(AssetProvider assetProvider)
        {
            _assetProvider = assetProvider;
        }

        [Route("asset/{id}")]
        public async Task<ActionResult> Index(string id)
        {
            var result = await _assetProvider.GetAssetAsync(id);
            if (result != null)
            {
                return View(AssetViewModel.Create(result));
            }

            return View("NotFound");
        }
    }
}