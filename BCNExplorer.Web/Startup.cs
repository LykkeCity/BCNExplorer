using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;

[assembly: OwinStartupAttribute(typeof(BCNExplorer.Web.Startup))]
namespace BCNExplorer.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //var settings = GeneralSettingsReader.ReadGeneralSettings<BaseSettings>(Dependencies.WebSiteSettings.ConnectionString);

            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();

            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "TempState",
                AuthenticationMode = AuthenticationMode.Passive
            });

            //app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);


            //app.UseCookieAuthentication(new CookieAuthenticationOptions
            //{
            //    ExpireTimeSpan = TimeSpan.FromHours(24),
            //    LoginPath = new PathString("/signin"),
            //});

            //app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            //{
            //    ClientId = settings.Authentication.ClientId,
            //    ClientSecret = settings.Authentication.ClientSecret,
            //    PostLogoutRedirectUri = settings.Authentication.PostLogoutRedirectUri,
            //    Authority = settings.Authentication.Authority,
            //    CallbackPath = new PathString("/auth"),
            //    RedirectUri = settings.Authentication.RedirectUri,
            //    ResponseType = "code",
            //    Scope = "email profile",
            //    UseTokenLifetime = true,

            //});
        }
    }
}
