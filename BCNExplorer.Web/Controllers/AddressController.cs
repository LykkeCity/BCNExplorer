using System.Threading.Tasks;
using System.Web.Mvc;
using BCNExplorer.Web.Models;
using Core.AddressService;
using Core.Asset;

namespace BCNExplorer.Web.Controllers
{
    public class AddressController:Controller
    {
        private readonly IAddressService _addressProvider;
        private readonly IAssetService _assetService;

        public AddressController(IAddressService addressProvider, IAssetService assetService)
        {
            _addressProvider = addressProvider;
            _assetService = assetService;
        }

        [Route("address/{id}")]
        public async Task<ActionResult> Index(string id)
        {
            var result = await _addressProvider.GetBalanceAsync(id);
            if (result != null)
            {
                return View(AddressViewModel.Create(result, await _assetService.GetAssetDefinitionDictionaryAsync()));
            }

            return View("NotFound");
        }
    }
}