using System.Threading.Tasks;
using System.Web.Mvc;
using BCNExplorer.Web.Models;
using Core.Block;
using Providers.Providers.Common;
using Providers.Providers.Ninja;
using Providers.TransportTypes.Ninja;
using SearchProvider = Providers.Providers.Common.SearchProvider;

namespace BCNExplorer.Web.Controllers
{
    public class SearchController : Controller
    {
        private readonly SearchProvider _searchProvider;
        private readonly IBlockService _blockService;

        public SearchController(SearchProvider searchProvider, IBlockService blockService)
        {
            _searchProvider = searchProvider;
            _blockService = blockService;
        }

        [Route("search")]
        public async Task<ActionResult> Search(string id)
        {
            id = (id ?? "").Trim();
            var type = await _searchProvider.GetTypeAsync(id);
            switch (type)
            {
                case SearchResultType.Block:
                {
                    return RedirectToAction("Index", "Block", new {id = id});
                }
                case SearchResultType.Transaction:
                {
                    return RedirectToAction("Index", "Transaction", new { id = id });
                }
                case SearchResultType.Address:
                {
                    return RedirectToAction("Index", "Address", new { id = id });
                }
                case SearchResultType.Asset:
                {
                    return RedirectToAction("Index", "Asset", new { id = id });
                }
                default:
                {
                    var lastBlock = await _blockService.GetLastBlockHeaderAsync();
                    return View("NotFound", LastBlockViewModel.Create(lastBlock));
                }
            }
        }
    }
}