using System.Web;
using System.Web.Optimization;

namespace BCNExplorer.Web
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/js/jquery").Include(
                "~/js/vendor/jquery.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/js/bootstrap").Include(
                "~/js/vendor/bootstrap.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/js/bootstrap-datetimepicker")
                .Include("~/js/vendor/bootstrap-datetimepicker.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/js/moment")
                .Include("~/js/vendor/moment.js"));

            bundles.Add(new ScriptBundle("~/bundles/js/fastclick").Include(
                "~/js/vendor/fastclick.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/js/clipboard").Include(
                "~/js/vendor/clipboard.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/js/common").Include(
                "~/js/app/common/*.js"));

            bundles.Add(new ScriptBundle("~/bundles/js/transaction").Include(
                "~/js/app/transaction/*.js"));

            bundles.Add(new ScriptBundle("~/bundles/js/address").Include(
                "~/js/app/address/*.js"));


            bundles.Add(new ScriptBundle("~/bundles/js/asset").Include(
                "~/js/app/asset/*.js"));

            bundles.Add(new ScriptBundle("~/bundles/js/offchain").Include(
                "~/js/app/offchain/*.js"));

            bundles.Add(new ScriptBundle("~/bundles/js/angular").Include(
                "~/js/vendor/angular.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/js/asset/directory").Include("~/js/app/asset/directory/*.js"));

            bundles.Add(new StyleBundle("~/bundles/css/bootstrap")
                .Include("~/css/bootstrap-custom.min.css", new CssRewriteUrlTransform()));

            bundles.Add(new Bundle("~/bundles/css/style")
                .Include("~/css/style.css", new CssRewriteUrlTransform()));
            
            //BundleTable.EnableOptimizations = true;
        }
    }
}
