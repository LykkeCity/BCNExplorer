using System.Threading.Tasks;
using System.Web.Mvc;
using BCNExplorer.Web.Models;
using Providers.Providers.Common;
using Providers.Providers.Ninja;
using Providers.TransportTypes.Ninja;
using SearchProvider = Providers.Providers.Common.SearchProvider;

namespace BCNExplorer.Web.Controllers
{
    public class SearchController : Controller
    {
        private readonly SearchProvider _searchProvider;
        private readonly BlockProvider _blockProvider;

        public SearchController(SearchProvider searchProvider, BlockProvider blockProvider)
        {
            _searchProvider = searchProvider;
            _blockProvider = blockProvider;
        }

        [Route("search")]
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
                    var lastBlock = await _blockProvider.GetLastBlockAsync();
                    return View("NotFound", LastBlockViewModel.Create(lastBlock));
                }
            }
        }
    }
}