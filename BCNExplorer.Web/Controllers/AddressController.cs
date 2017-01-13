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
            var result = await _addressProvider.GetMainInfoAsync(id);
            if (result != null)
            {
                return View(AddressMainInfoViewModel.Create(result));
            }

            return View("NotFound");
        }

        [Route("address/balance/{id}")]
        public async Task<ActionResult> Balance(string id)
        {
            var result = await _addressProvider.GetBalanceAsync(id);
            if (result != null)
            {
                return View(AddressBalanceViewModel.Create(result, await _assetService.GetAssetDefinitionDictionaryAsync()));
            }

            return View("NotFound");
        }

        [Route("address/transactions/{id}")]
        public async Task<ActionResult> Transactions(string id)
        {
            return View(AddressTransactionsViewModel.Create(await _addressProvider.GetTransactions(id)));
        } 
    }
}