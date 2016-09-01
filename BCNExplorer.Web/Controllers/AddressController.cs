using System.Threading.Tasks;
using System.Web.Mvc;
using BCNExplorer.Web.Models;
using Providers.Providers;
using Providers.Providers.Ninja;

namespace BCNExplorer.Web.Controllers
{
    public class AddressController:Controller
    {
        private readonly NinjaAddressProvider _ninjaAddressProvider;

        public AddressController(NinjaAddressProvider ninjaAddressProvider)
        {
            _ninjaAddressProvider = ninjaAddressProvider;
        }

        [Route("address/{id}")]
        public async Task<ActionResult> Index(string id)
        {
            var result = await _ninjaAddressProvider.GetAddressAsync(id);
            if (result != null)
            {
                return View(AddressViewModel.Create(result));
            }

            return View("NotFound");
        }
    }
}