using System;
using AzureRepositories;
using BCNExplorer.Web.App_Start;
using Core.Settings;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;

[assembly: OwinStartupAttribute(typeof(BCNExplorer.Web.Startup))]
namespace BCNExplorer.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var settings = GeneralSettingsReader.ReadGeneralSettings<BaseSettings>(Dependencies.WebSiteSettings.ConnectionString);

            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                ExpireTimeSpan = TimeSpan.FromHours(24),
                LoginPath = new PathString("/signin"),
            });

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                ClientId = settings.Authentication.ClientId,
                ClientSecret = settings.Authentication.ClientSecret,
                PostLogoutRedirectUri = settings.Authentication.PostLogoutRedirectUri,
                Authority = settings.Authentication.Authority,
                CallbackPath = new PathString("/auth"),
                RedirectUri = settings.Authentication.RedirectUri,
                ResponseType = "code",
                Scope = "email profile",
                UseTokenLifetime = true,
                
            });
        }
    }
}
