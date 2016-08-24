using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using NinjaProviders;

namespace BCNExplorer.Web.Controllers
{
    public class BlockController : Controller
    {
        private readonly NinjaBlockProvider _ninjaBlockProvider;

        public BlockController(NinjaBlockProvider ninjaBlockProvider)
        {
            _ninjaBlockProvider = ninjaBlockProvider;
        }
        
        [Route("block/{id}")]
        public async Task<ActionResult> Index(string id)
        {
            var result = await _ninjaBlockProvider.GetAsync(id);
            if (result == null)
                return HttpNotFound();

            return View();
        }
    }
}