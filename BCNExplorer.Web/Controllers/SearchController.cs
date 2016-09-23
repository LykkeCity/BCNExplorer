using System.Threading.Tasks;
using System.Web.Mvc;
using BCNExplorer.Web.Models;
using Providers.Providers.Common;
using Providers.Providers.Ninja;
using Providers.TransportTypes.Ninja;

namespace BCNExplorer.Web.Controllers
{
    public class SearchController : Controller
    {
        private readonly SearchProvider _searchProvider;
        private readonly NinjaBlockProvider _ninjaBlockProvider;

        public SearchController(SearchProvider searchProvider, NinjaBlockProvider ninjaBlockProvider)
        {
            _searchProvider = searchProvider;
            _ninjaBlockProvider = ninjaBlockProvider;
        }

        public async Task<ActionResult> Search(string id)
        {
            id = (id ?? "").Trim();
            var type = await _searchProvider.GetTypeAsync(id);
            switch (type)
            {
                case NinjaType.Block:
                {
                    return RedirectToAction("Index", "Block", new {id = id});
                }
                case NinjaType.Transaction:
                {
                    return RedirectToAction("Index", "Transaction", new { id = id });
                }
                case NinjaType.Address:
                {
                    return RedirectToAction("Index", "Address", new { id = id });
                }
                case NinjaType.Asset:
                {
                    return RedirectToAction("Index", "Asset", new { id = id });
                }
                default:
                {
                    var lastBlock = await _ninjaBlockProvider.GetLastBlockAsync();
                    return View("NotFound", LastBlockViewModel.Create(lastBlock));
                }
            }
        }
    }
}