using System.Web.Mvc;
using BCNExplorer.Web.Models;

namespace BCNExplorer.Web.Controllers
{
    public class AddressController:Controller
    {
        [Route("address/{id}")]
        public ActionResult Index()
        {
            return View(new AddressViewModel());
        }
    }
}