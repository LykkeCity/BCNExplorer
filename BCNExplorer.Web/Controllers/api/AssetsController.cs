using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using BCNExplorer.Web.Models;
using Core.Asset;
using Core.AssetBlockChanges.Mongo;

namespace BCNExplorer.Web.Controllers.api
{
    public class AssetsController : ApiController
    {
        private readonly IAssetService _assetService;
        private readonly IAssetBalanceChangesRepository _balanceChangesRepository;

        public AssetsController(IAssetService assetService, 
            IAssetBalanceChangesRepository balanceChangesRepository)
        {
            _assetService = assetService;
            _balanceChangesRepository = balanceChangesRepository;
        }

        public async Task<IEnumerable<AssetDirectoryViewModel.Asset>> Get()
        {
            var assetDefinitions = _assetService.GetAssetDefinitionsAsync();
            var assetCoinholdersIndexes = _assetService.GetAssetCoinholdersIndexAsync();
            var assetScores = _assetService.GetAssetScoreDictionaryAsync();

            await Task.WhenAll(assetCoinholdersIndexes, assetDefinitions, assetScores);

            var result = AssetDirectoryViewModel.Create(assetDefinitions.Result, assetCoinholdersIndexes.Result, assetScores.Result);
            return result.Assets;
        }
    }
}
