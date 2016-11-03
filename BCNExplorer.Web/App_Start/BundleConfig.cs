using System.Web;
using System.Web.Optimization;

namespace BCNExplorer.Web
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            //bundles.Add(new ScriptBundle("~/bundles/js/jquery").Include(
            //            "~/Scripts/jquery-{version}.js"));
            
            //bundles.Add(new ScriptBundle("~/bundles/js/bootstrap").Include(
            //          "~/Scripts/bootstrap.js",
            //          "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/js/common").Include(
                      "~/js/app/common/*.js"));

            bundles.Add(new ScriptBundle("~/bundles/js/transaction").Include(
                      "~/js/app/transaction/*.js"));

            bundles.Add(new ScriptBundle("~/bundles/js/address").Include(
          "~/js/app/address/*.js"));

            //bundles.Add(new StyleBundle("~/Content/maincss")
            //    .Include("~/Content/bootstrap.css", new CssRewriteUrlTransform())
            //    .Include("~/Content/site.css", new CssRewriteUrlTransform())
            //    .Include("~/Content/font-awesome.css", new CssRewriteUrlTransform()));
        }
    }
}
