using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using BCNExplorer.Web.Models;
using NinjaProviders;
using NinjaProviders.Providers;

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
        [OutputCache(Duration = 10*60, VaryByParam = "*")]
        public async Task<ActionResult> Index(string id)
        {
            var ninjaBlock = await _ninjaBlockProvider.GetAsync(id);

            if (ninjaBlock != null)
            {
                var result = BlockViewModel.Create(ninjaBlock);

                return View(result);
            }

            return View("NotFound");
        }
    }
}