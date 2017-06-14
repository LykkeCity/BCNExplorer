using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using BCNExplorer.Web.Models;
using Core.Asset;
using Core.AssetBlockChanges.Mongo;

namespace BCNExplorer.Web.Controllers.api
{
    public class AssetsTransactionsController : ApiController
    {
        private readonly IAssetService _assetService;
        private readonly IAssetBalanceChangesRepository _balanceChangesRepository;

        public AssetsTransactionsController(IAssetBalanceChangesRepository balanceChangesRepository, 
            IAssetService assetService)
        {
            _balanceChangesRepository = balanceChangesRepository;
            _assetService = assetService;
        }

        public async Task<IEnumerable<AssetTransactionViewModel>> Get(string id)
        {
            var asset = await _assetService.GetAssetAsync(id);

            if (asset != null)
            {
                var txs = await _balanceChangesRepository.GetTransactionsAsync(asset.AssetIds);

                return txs.Select(AssetTransactionViewModel.Create);
            }

            return Enumerable.Empty<AssetTransactionViewModel>();
        }
    }
}