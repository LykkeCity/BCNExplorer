using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security.Cookies;

namespace BCNExplorer.Web.Controllers
{
    public class SecurityController : Controller
    {
        [Route("auth")]
        public async Task<ActionResult> Auth()
        {
            //await HttpContext.GetOwinContext().Authentication.AuthenticateAsync("Cookies");
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public ActionResult SignIn()
        {
            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public ActionResult SignOut()
        {
            if (User.Identity.IsAuthenticated)
            {
                var authManager = HttpContext.GetOwinContext().Authentication;

                authManager.SignOut("Cookies");
                authManager.SignOut("OpenIdConnect");

            }
            return RedirectToAction("Index", "Home");
        }
    }
}