using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace TheBookingPlatform
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
          name: "BusinessRoute",
            url: "{businessName}/Booking/{action}",
          defaults: new { controller = "Booking", action = "Index" }
      );
            routes.MapRoute(
    name: "PushNotification",
    url: "PushNotification/{action}/{id}",
    defaults: new { controller = "PushNotification", action = "GetVapidPublicKey", id = UrlParameter.Optional }
);
            routes.MapRoute(
          name: "GiftCardRoute",
            url: "{businessName}/OnlineGiftCard/{action}",
          defaults: new { controller = "OnlineGiftCard", action = "Index" }
      );
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Account", action = "Login", id = UrlParameter.Optional }
            );
        }
    }
}
