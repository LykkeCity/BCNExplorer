using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BCNExplorer.Web.Models;
using Core.Asset;
using Core.AssetBlockChanges.Mongo;
using Core.Block;
using Providers.Helpers;
using Services.MainChain;

namespace BCNExplorer.Web.Controllers
{
    public class AssetController : Controller
    {
        private readonly IAssetService _assetService;
        private readonly IAssetBalanceChangesRepository _balanceChangesRepository;
        private readonly IBlockService _blockService;

        private readonly CachedMainChainService _mainChainService;

        public AssetController(IAssetService assetService, 
            IAssetBalanceChangesRepository balanceChangesRepository, 
            IBlockService blockService,
            CachedMainChainService mainChainService)
        {
            _assetService = assetService;
            _balanceChangesRepository = balanceChangesRepository;
            _blockService = blockService;
            _mainChainService = mainChainService;
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
        public ActionResult AssetDirectiory()
        {
            return View();
        }
        #region owners

        [OutputCache(Duration = 1 * 60, VaryByParam = "*")]
        public Task<ActionResult> Owners(string id)
        {
            return _OwnersInner(id);
        }

        [OutputCache(Duration = 60 * 60, VaryByParam = "*")]
        public Task<ActionResult> OwnersHistory(string id, int? at)
        {
            return _OwnersInner(id, at);
        }
        
        public async Task<ActionResult> OwnersHistoryByDate(DateTime at, string id)
        {
            var mainChain = await _mainChainService.GetMainChainAsync();

            var block = mainChain.GetClosestToTimeBlock(at);
            
            return RedirectToAction("OwnersHistory", new { id = id, at = block?.Height ?? 0 });
        }
        
        private async Task<ActionResult> _OwnersInner(string id, int? at = null)
        {
            var result = await GetOwnersAsync(id, at);

            if (result != null)
            {
                return View("Owners", result);
            }

            return View("NotFound");
        }

        private async Task<AssetCoinholdersViewModel> GetOwnersAsync(string id, int? at)
        {
            var asset = await _assetService.GetAssetAsync(id);

            if (asset != null)
            {
                var queryOpts = at != null ? QueryOptions.Create().To(at.Value) : null;
                var summaryTask = _balanceChangesRepository.GetSummaryAsync(queryOpts, asset.AssetIds.ToArray());
                var addressChangesTask = _balanceChangesRepository.GetBlocksWithChanges(asset.AssetIds);
                var lastBlockTask = _blockService.GetLastBlockHeaderAsync();
                Task<IDictionary<string, double>> addressChangesAtBlockTask;
                Task<IBlockHeader> atBlockInfoTask;
                if (at != null)
                {
                    atBlockInfoTask = _blockService.GetBlockHeaderAsync(at.ToString());
                    addressChangesAtBlockTask = _balanceChangesRepository.GetAddressQuantityChangesAtBlock(at.Value, asset.AssetIds.ToArray());
                }
                else
                {
                    atBlockInfoTask = Task.FromResult((IBlockHeader)null);
                    addressChangesAtBlockTask = Task.FromResult((IDictionary<string, double>)new Dictionary<string, double>());
                }

                await Task.WhenAll(addressChangesAtBlockTask, summaryTask, addressChangesTask, lastBlockTask, atBlockInfoTask);

                var result = AssetCoinholdersViewModel.Create(
                    AssetViewModel.Create(asset),
                    summaryTask.Result,
                    at,
                    addressChangesAtBlockTask.Result,
                    addressChangesTask.Result,
                    lastBlockTask.Result,
                    atBlockInfoTask.Result
                    );

                return result;
            }

            return null;
        }

        [OutputCache(Duration = 1 * 60, VaryByParam = "*")]
        public async Task<ActionResult> Transactions(string id)
        {
            var asset = await _assetService.GetAssetAsync(id);

            if (asset != null)
            {
                var txs = await _balanceChangesRepository.GetTransactionsAsync(asset.AssetIds);

                var txList = new TransactionIdList(txs.Select(p => p.Hash));

                return View("~/Views/Transaction/TransactionIdList.cshtml", txList);
            }


            return View("NotFound");
        }

        [OutputCache(Duration = 1 * 60, VaryByParam = "*")]
        public async Task<ActionResult> OwnersToCsv(string id, int at)
        {
            var result = await GetOwnersAsync(id, at);

            if (result != null)
            {
                return File(result.ToCsv(), "text/csv", $"Coinholders-{result.Asset.NameShort}-{at}.csv");
            }

            return View("NotFound");
        }

        #endregion
    }
}