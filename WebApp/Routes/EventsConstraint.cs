using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace WebApp.Routes
{
    public class EventsConstraint : IRouteConstraint
    {
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {

            using (var db = new DataContainer())
            {
                var eventUrls = db.Events.Select(x => x.EventFriendlyUrl).ToArray();
                db.Database.Connection.Close();
                return eventUrls.Contains(values["eventurl"]);
            }
        }
    }
}