using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QBitNinja.Client;
using QBitNinja.Client.Models;

namespace BCNExplorer.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var client = new QBitNinjaClient("http://testnet-ninja.azurewebsites.net/");

            var bl = client.GetBlock(new BlockFeature(924319)).Result;

            return View();
        }
    }
}