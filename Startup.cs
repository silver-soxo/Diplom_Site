using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DiplomSite.Startup))]
namespace DiplomSite
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
