using System.Collections.Generic;
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
            var assetDefinitions = _assetService.GetAssetDefinitionsAsync();
            var assetCoinholdersIndexes = _assetService.GetAssetCoinholdersIndexAsync();
            
            await Task.WhenAll(assetCoinholdersIndexes, assetDefinitions);

            var result = AssetDirectoryViewModel.Create(assetDefinitions.Result, assetCoinholdersIndexes.Result);

            return View(result);
        }


        [OutputCache(Duration = 1 * 60, VaryByParam = "*")]
        [Route("asset/{id}/owners")]
        public Task<ActionResult> Owners(string id)
        {
            return _OwnersInner(id);
        }

        [OutputCache(Duration = 60 * 60, VaryByParam = "*")]
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
                var queryOpts = at != null ? QueryOptions.Create().To(at.Value) : null;
                var summaryTask = _balanceChangesRepository.GetSummaryAsync(queryOpts, asset.AssetIds.ToArray());
                var addressChangesTask = _balanceChangesRepository.GetBlocksWithChanges(asset.AssetIds);
                Task<IDictionary<string, double>> addressChangesAtBlockTask;
                if (at != null)
                {
                    addressChangesAtBlockTask = _balanceChangesRepository.GetAddressQuantityChangesAtBlock(at.Value, asset.AssetIds.ToArray());
                }
                else
                {
                    addressChangesAtBlockTask = Task.FromResult((IDictionary<string, double>)new Dictionary<string, double>());
                }

                await Task.WhenAll(addressChangesAtBlockTask, summaryTask, addressChangesTask);
                
                var result = AssetCoinholdersViewModel.Create(
                    AssetViewModel.Create(asset), 
                    summaryTask.Result, 
                    at, 
                    addressChangesAtBlockTask.Result, 
                    addressChangesTask.Result);

                return View("Owners", result);
            }

            return View("NotFound");
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
    }
}