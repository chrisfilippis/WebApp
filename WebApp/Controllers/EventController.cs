using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApp.Controllers
{
    public class EventController : Controller
    {
        public ActionResult Index(string eventurl)
        {
            if (!Utilities.Helpers.IsLoggedIn(HttpContext))
            {
                return RedirectToAction("Index", "Home");
            }

            using (var db = new DataContainer())
            {
                var Event = db.Events.FirstOrDefault(x => x.EventFriendlyUrl == eventurl);
                if (Event == null)
                    return HttpNotFound();

                return View(Event);
            }
        }
    }
}
