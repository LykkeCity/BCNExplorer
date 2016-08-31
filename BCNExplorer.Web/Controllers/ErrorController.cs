using System.Web.Mvc;

namespace BCNExplorer.Web.Controllers
{
    public class ErrorController:Controller
    {
        [Route("notfound")]
        public ActionResult NotFound()
        {
            return View();
        }
    }
}