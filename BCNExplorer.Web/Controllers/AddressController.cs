using System.Threading.Tasks;
using System.Web.Mvc;
using BCNExplorer.Web.Models;
using Providers.Providers.Asset;
using Providers.Providers.Ninja;

namespace BCNExplorer.Web.Controllers
{
    public class AddressController:Controller
    {
        private readonly NinjaAddressProvider _ninjaAddressProvider;
        private readonly AssetProvider _assetProvider;

        public AddressController(NinjaAddressProvider ninjaAddressProvider, AssetProvider assetProvider)
        {
            _ninjaAddressProvider = ninjaAddressProvider;
            _assetProvider = assetProvider;
        }

        [Route("address/{id}")]
        public async Task<ActionResult> Index(string id)
        {
            var result = await _ninjaAddressProvider.GetAddressAsync(id);
            if (result != null)
            {
                return View(AddressViewModel.Create(result, await _assetProvider.GetAssetDictionaryAsync()));
            }

            return View("NotFound");
        }
    }
}