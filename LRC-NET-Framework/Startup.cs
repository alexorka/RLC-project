using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(LRC_NET_Framework.Startup))]
namespace LRC_NET_Framework
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
