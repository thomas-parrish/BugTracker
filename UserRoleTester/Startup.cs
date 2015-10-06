using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(UserRoleTester.Startup))]
namespace UserRoleTester
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
