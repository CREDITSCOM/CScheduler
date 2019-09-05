using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CScheduler.Startup))]
namespace CScheduler
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
