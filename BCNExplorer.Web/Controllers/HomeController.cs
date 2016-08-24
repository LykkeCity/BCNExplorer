using System.Web.Mvc;

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