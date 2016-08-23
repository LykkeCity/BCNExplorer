using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AzureRepositories;
using BCNExplorer.Web.App_Start;
using Core.Settings;
using QBitNinja.Client;
using QBitNinja.Client.Models;

namespace BCNExplorer.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
           return View();
        }
    }
}