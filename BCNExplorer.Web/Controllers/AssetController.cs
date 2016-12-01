using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BCNExplorer.Web.Models;
using Core.Asset;
using Core.AssetBlockChanges.Mongo;

namespace BCNExplorer.Web.Controllers
{
    public class AssetController : Controller
    {
        private readonly IAssetService _assetService;
        private readonly IAssetBalanceChangesRepository _balanceChangesRepository;

        public AssetController(IAssetService assetService, 
            IAssetBalanceChangesRepository balanceChangesRepository)
        {
            _assetService = assetService;
            _balanceChangesRepository = balanceChangesRepository;
        }

        [Route("asset/{id}")]
        public async Task<ActionResult> Index(string id)
        {
            var result = await _assetService.GetAssetAsync(id);
            if (result != null)
            {
                return View(AssetViewModel.Create(result));
            }

            return View("NotFound");
        }

        [Route("assets")]
        public async Task<ActionResult> AssetDirectiory()
        {
            var result = (await _assetService.GetAssetsAsync()).Select(AssetViewModel.Create);

            return View(result);
        }


        //[OutputCache(Duration = 1 * 60, VaryByParam = "*")]
        [Route("asset/{id}/owners")]
        public Task<ActionResult> Owners(string id)
        {
            return _OwnersInner(id);
        }

        //[OutputCache(Duration = 60*60, VaryByParam = "*")]
        [Route("asset/{id}/owners/{at}")]
        public Task<ActionResult> OwnersHistory(string id, int? at)
        {
            return _OwnersInner(id, at);
        }

        private async Task<ActionResult> _OwnersInner(string id, int? at = null)
        {
            var asset = await _assetService.GetAssetAsync(id);

            if (asset != null)
            {
                var result = AssetCoinholdersViewModel.Create(
                    AssetViewModel.Create(asset),
                    await _balanceChangesRepository.GetSummaryAsync(at, asset.AssetIds.ToArray()));

                return View("Owners", result);
            }

            return View("NotFound");
        } 
    }
}