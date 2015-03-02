using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApp.Base;
using WebApp.Models;
using WebApp.Utilities;

namespace WebApp.Controllers
{
    public class AnnouncementsController : BaseController
    {
        //
        // GET: /Announcements/
        public ActionResult Index()
        {
            if (!Utilities.Helpers.IsLoggedIn(HttpContext))
            {
                return RedirectToAction("Index", "Home");
            }

            using (var db = new DataContainer())
            {
                var announcements = db.Announcements.Take(30)
                    .Select(x => new AnnouncementObject
                    {
                        Content = x.annContent,
                        Id = x.annId,
                        Title = x.annTitle,
                        PublishedDate = x.annPublishedDate,
                        UserName = x.User.userName
                    }).ToArray();

                return View(announcements);
            }
        }

        [HttpPost]
        public JsonResult AddAnnouncement(FormCollection form)
        {
            if (form["announcement"] == null || string.IsNullOrWhiteSpace(form["announcement"].ToString()))
            {
                return new JsonResult { Data = new { success = false, message = "Invalid Text" } };
            }

            var uid = 0;

            if (HttpContext.Session["UserId"] == null || !Int32.TryParse(HttpContext.Session["UserId"].ToString(), out uid))
            {
                return new JsonResult { Data = new { success = false, message = "Error Occurred" } };
            }

            var content = form["announcement"].ToString();

            using (var db = new DataContainer())
            {
                var announcement = new Announcements
                {
                    annContent = content,
                    annPublishedDate = DateTime.Now,
                    annTitle = "",
                    annUserId = uid
                };

                db.Announcements.Add(announcement);
                db.SaveChanges();

                Activities.AddAnnouncement(uid, announcement.annId, LanguageId);

                return new JsonResult { Data = new { success = true, message = "" } };
            }

        }

    }
}
