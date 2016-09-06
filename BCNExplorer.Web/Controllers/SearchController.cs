using System.Threading.Tasks;
using System.Web.Mvc;
using Providers.Providers.Common;
using Providers.TransportTypes.Ninja;

namespace BCNExplorer.Web.Controllers
{
    public class SearchController : Controller
    {
        private readonly SearchProvider _searchProvider;

        public SearchController(SearchProvider searchProvider)
        {
            _searchProvider = searchProvider;
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
                    return View("NotFound");
                }
            }
        }
    }
}