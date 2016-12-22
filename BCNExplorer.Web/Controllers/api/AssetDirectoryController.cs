using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using BCNExplorer.Web.Models;
using Core.Asset;

namespace BCNExplorer.Web.Controllers
{
    public class AssetsController : ApiController
    {
        private readonly IAssetService _assetService;

        public AssetsController(IAssetService assetService)
        {
            _assetService = assetService;
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
