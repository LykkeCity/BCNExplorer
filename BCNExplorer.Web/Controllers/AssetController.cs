﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BCNExplorer.Web.Models;
using Core.AssetBlockChanges.Mongo;
using Providers.Providers.Asset;

namespace BCNExplorer.Web.Controllers
{
    public class AssetController : Controller
    {
        private readonly AssetProvider _assetProvider;
        private readonly IAssetBalanceChangesRepository _balanceChangesRepository;

        public AssetController(AssetProvider assetProvider, 
            IAssetBalanceChangesRepository balanceChangesRepository)
        {
            _assetProvider = assetProvider;
            _balanceChangesRepository = balanceChangesRepository;
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

        [Route("assets")]
        public async Task<ActionResult> AssetDirectiory()
        {
            var result = (await _assetProvider.GetAssetsAsync()).Select(AssetViewModel.Create);

            return View(result);
        }

        [Route("asset/{id}/owners")]
        public async Task<ActionResult> Owners(string id, int? at)
        {
            var asset = await _assetProvider.GetAssetAsync(id);
            
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