using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using WebApp.Routes;

namespace WebApp
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "EventsLanguageRoute",
                url: "{language}/events/{eventurl}/",
                defaults: new { language = "gr", controller = "Event", action = "Index" },
                constraints: new { language = new LanguageConstraint("gr", "en"), eventurl = new EventsConstraint() }
            );

            routes.MapRoute(
                name: "EventsRoute",
                url: "events/{eventurl}/",
                defaults: new { language = "gr", controller = "Event", action = "Index" },
                constraints: new { language = new LanguageConstraint("gr", "en"), eventurl = new EventsConstraint() }
            );

            routes.MapRoute(
                name: "LanguageRoute",
                url: "{language}/{controller}/{action}/{id}",
                defaults: new { language = "gr", controller = "Home", action = "Index", id = UrlParameter.Optional },
                constraints: new { language = new LanguageConstraint("gr", "en") }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { language = "gr", controller = "Home", action = "Index", id = UrlParameter.Optional },
                constraints: new { language = new LanguageConstraint("gr", "en") }
            );

            //Just Controller
            routes.MapRoute(
                name: "Controller",
                url: "{language}/{controller}",
                defaults: new { language = "gr", controller = "Home", action = "Index", id = UrlParameter.Optional },
                constraints: new { language = new LanguageConstraint("gr", "en") }
            );

            //homepage
            routes.MapRoute(
                name: "Home",
                url: "{language}",
                defaults: new { language = "gr", controller = "Home", action = "Index", id = UrlParameter.Optional },
                constraints: new { language = new LanguageConstraint("gr", "en") }
            );

        }
    }
}