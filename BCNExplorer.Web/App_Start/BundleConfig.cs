using System.Web;
using System.Web.Optimization;

namespace BCNExplorer.Web
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/js/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));
            
            bundles.Add(new ScriptBundle("~/bundles/js/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/bundles/css/main").Include(
                      "~/Content/bootstrap.css",
                       "~/Content/site.css",
                      "~/Content/media.css",
                      "~/Content/font-awesome.css"
                     ));
        }
    }
}
