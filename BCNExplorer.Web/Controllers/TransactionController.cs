using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BCNExplorer.Web.Controllers
{
    public class TransactionController:Controller
    {
        public ActionResult Index(string id)
        {
            return HttpNotFound();
        }
    }
}