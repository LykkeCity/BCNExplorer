﻿using System.Threading.Tasks;
using System.Web.Mvc;
using BCNExplorer.Web.Models;
using NinjaProviders.Providers;

namespace BCNExplorer.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly NinjaBlockProvider _ninjaBlockProvider;

        public HomeController(NinjaBlockProvider ninjaBlockProvider)
        {
            _ninjaBlockProvider = ninjaBlockProvider;
        }

        public async Task<ActionResult> Index()
        {
            var lastBlock = await _ninjaBlockProvider.GetLastBlockAsync();

            return View(LastBlockViewModel.Create(lastBlock));
        }
    }
}