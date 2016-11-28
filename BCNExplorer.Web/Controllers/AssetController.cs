using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BCNExplorer.Web.Models;
using Core.Asset;
using Core.AssetBlockChanges.Mongo;
using Providers.Providers.Asset;
using Services.Asset;

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

        [Route("asset/{id}/owners")]
        public async Task<ActionResult> Owners(string id, int? at)
        {
            var asset = await _assetService.GetAssetAsync(id);
            
            if (asset != null)
            {
                var result = AssetCoinholdersViewModel.Create(
                    AssetViewModel.Create(asset),
                    await _balanceChangesRepository.GetSummaryAsync(at, asset.AssetIds.ToArray()));

                return View(result);
            }

            return View("NotFound");
        }
    }
}