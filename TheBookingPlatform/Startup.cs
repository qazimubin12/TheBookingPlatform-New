using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin;
using Owin;
using Stripe;
using System.Web.Configuration;

[assembly: OwinStartupAttribute(typeof(TheBookingPlatform.Startup))]
namespace TheBookingPlatform
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);


        }


    }
}
