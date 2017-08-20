using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using BCNExplorer.Web.App_Start;
using BCNExplorer.Web.Controllers;

namespace BCNExplorer.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            var resolver = Dependencies.CreateDepencencyResolver();
            DependencyResolver.SetResolver(resolver);
            GlobalConfiguration.Configuration.DependencyResolver = resolver;

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_EndRequest()
        {
            if (Context.Response.StatusCode == 404)
            {
                Response.Clear();

                var rd = new RouteData();

                rd.Values["controller"] = "Error";
                rd.Values["action"] = "NotFound";

                IController c = new ErrorController();
                c.Execute(new RequestContext(new HttpContextWrapper(Context), rd));
            }

            if (Context.Response.StatusCode == 500)
            {
                if (!Context.Request.IsLocal)
                {
                    Response.Clear();

                    var rd = new RouteData();

                    rd.Values["controller"] = "Error";
                    rd.Values["action"] = "Server";

                    IController c = new ErrorController();
                    c.Execute(new RequestContext(new HttpContextWrapper(Context), rd));
                }
            }
        }
    }
}
