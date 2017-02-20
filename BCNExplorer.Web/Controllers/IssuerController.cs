using System.Threading.Tasks;
using System.Web.Mvc;
using BCNExplorer.Web.Models;
using Common;
using Core.Asset;

namespace BCNExplorer.Web.Controllers
{
    public class IssuerController: Controller
    {
        private readonly IAssetService _assetService;

        public IssuerController(IAssetService assetService)
        {
            _assetService = assetService;
        }

        [Route("issuer/{id}")]
        public async Task<ActionResult> IssuerDetails(string id)
        {
            var issuerName = id.IsBase64() ? id.Base64ToString() : null;
            if (issuerName != null)
            {
                var assetDefinitions = _assetService.GetAssetDefinitionsAsync(issuerName);
                var assetCoinholdersIndexes = _assetService.GetAssetCoinholdersIndexAsync();
                var assetScores = _assetService.GetAssetScoreDictionaryAsync();

                await Task.WhenAll(assetCoinholdersIndexes, assetDefinitions, assetScores);

                var result = IssuerViewModel.Create(issuerName,
                    assetDefinitions.Result, assetCoinholdersIndexes.Result,
                    assetScores.Result);

                if (result != null)
                {
                    return View(result);
                }
            }

            return View("NotFound");
        } 
    }
}