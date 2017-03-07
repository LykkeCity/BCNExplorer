using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.SessionState;
using BCNExplorer.Web.Models;
using Core.Block;
using Core.SearchService;
using Services.Search;

namespace BCNExplorer.Web.Controllers
{
    [SessionState(SessionStateBehavior.Disabled)]
    public class SearchController : Controller
    {
        private readonly ISearchService _searchService;
        private readonly IBlockService _blockService;

        public SearchController(ISearchService searchService, IBlockService blockService)
        {
            _searchService = searchService;
            _blockService = blockService;
        }

        [Route("search")]
        public async Task<ActionResult> Search(string id)
        {
            id = (id ?? "").Trim();
            var type = await _searchService.GetTypeAsync(id);
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