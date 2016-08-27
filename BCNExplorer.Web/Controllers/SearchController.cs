﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using NinjaProviders.Providers;
using NinjaProviders.TransportTypes;

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
                default:
                {
                    return HttpNotFound();
                }
            }
        }
    }
}