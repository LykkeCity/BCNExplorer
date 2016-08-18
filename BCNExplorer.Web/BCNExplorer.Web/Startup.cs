using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BCNExplorer.Web.Startup))]
namespace BCNExplorer.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
        }
    }
}
