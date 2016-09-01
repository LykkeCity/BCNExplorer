using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Providers.Providers;
using Providers.Providers.Ninja;
using Providers.TransportTypes;
using Providers.TransportTypes.Ninja;

namespace BCNExplorer.Web.Controllers
{
    public class SearchController : Controller
    {
        private readonly NinjaSearchProvider _ninjaSearchProvider;

        public SearchController(NinjaSearchProvider ninjaSearchProvider)
        {
            _ninjaSearchProvider = ninjaSearchProvider;
        }

        public async Task<ActionResult> Search(string id)
        {
            var type = await _ninjaSearchProvider.GetTypeAsync(id);
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
                default:
                {
                    return View("NotFound");
                }
            }
        }
    }
}