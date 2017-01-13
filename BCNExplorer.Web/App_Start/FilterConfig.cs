using System.Web;
using System.Web.Mvc;
using AzureRepositories;
using BCNExplorer.Web.App_Start;
using Core.Settings;

namespace BCNExplorer.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new ErrorHandler.CustomHandleErrorAttribute());

            var settings = GeneralSettingsReader.ReadGeneralSettings<BaseSettings>(Dependencies.WebSiteSettings.ConnectionString);
            if (!settings.DisableRedirectToHttps)
            {
                filters.Add(new RequireHttpsAttribute());
            }
        }
    }
}
