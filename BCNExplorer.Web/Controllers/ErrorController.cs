using System.Web.Mvc;

namespace BCNExplorer.Web.Controllers
{
    public class ErrorController:Controller
    {
        public ActionResult NotFound()
        {
            return View();
        }
    }
}