using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApp.Base;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class HomeController : BaseController
    {
        [HttpGet]
        [ActionName("Index")]
        //[OutputCache(Duration = 86400, VaryByParam = "language")]
        public ActionResult Index()
        {
            var la = LanguageId;
            var session = HttpContext.Session;
            if (session["UserId"] != null)
            {
                return RedirectToAction("Index", "List");
            };

            return View();
        }

        [HttpPost]
        public ActionResult Index(Login model)
        {
            if (ModelState.IsValid)
            {

                if (model.IsValid(model.Email, model.Password))
                {
                    using (var db = new DataContainer())
                    {
                        var user = db.Users.Where(x => x.userEmail == model.Email && !(x.userIsAnonymous ?? false) && x.userIsActive).FirstOrDefault();

                        if (user != null)
                        {
                            var hash = Utilities.Helpers.GetHashedPassword(model.Password, user.userSalt);
                            var pass = hash[1];

                            if (user.userIsNew ?? false)
                            {
                                user.userIsNew = false;
                                db.SaveChanges();
                            }

                            if (user.userPassword == pass)
                            {
                                var session = HttpContext.Session;
                                user.userIsNew = false;
                                user.userVisitDate = DateTime.Now;
                                db.SaveChanges();

                                session.Add("UserId", user.userId);
                                session.Add("UserName", user.userName);
                                session.Add("UserEmail", user.userEmail);
                                session.Add("UserGroupId", user.userGroupId);
                                return RedirectToAction("Welcome", "Home");
                            }

                            else
                            {
                                ModelState.AddModelError("Invalid Password", "error");
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("Invalid Password", "error");
                        }

                    }
                }
                else
                {
                    ModelState.AddModelError("", "error");
                }
            }

            return View(model);

        }

        [HttpGet]
        public ActionResult Welcome()
        {
            var userId = 0;

            if (!Utilities.Helpers.IsLoggedIn(HttpContext, out userId))
            {
                return RedirectToAction("Index", "Home");
            }

            using (var db = new DataContainer())
            {
                var user = db.Users.Where(x => x.userId == userId).FirstOrDefault();
                var userProjects = user.UserProjects.Select(x => x.Projects.prId).ToArray();

                var activities = db.Activities
                    .Where(x => (userProjects.Contains(x.activityProjectId ?? 0) || x.activityAnnouncementId != null)).OrderByDescending(x => x.activityDatetime)
                    .Take(15)
                    .ToArray();

                db.Database.Connection.Dispose();
                db.Database.Connection.Close();

                return View(new Welcome { ActivityItems = ManipulateActivityItems(activities).ToArray(), User = user });
            }
        }

        public ActionResult Remind()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Remind(Remind model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new DataContainer())
                {
                    var user = db.Users.FirstOrDefault(x => x.userEmail == model.Email);
                    if (user != null)
                    {
                        var newPassword = Utilities.UserUtilities.ResetPassword(user, db);

                        var emailSent = Utilities.EmailSender.SendEmail("", user.userEmail, "NewPass", "Name", string.Format("new pass is {0}", newPassword));

                        if (emailSent)
                        {
                            db.SaveChanges();
                            db.Database.Connection.Close();
                            return RedirectToAction("Index", "Home", new System.Web.Routing.RouteValueDictionary() { { "language", Utilities.LanguageResolver.GetLanguageCode(LanguageId) } });
                        }
                    }

                    ModelState.AddModelError("Email", "Invalid_Email");

                }
            }

            return View(model);
        }

        [NonAction]
        private IEnumerable<Activity> ManipulateActivityItems(Activity[] items)
        {
            using (var db = new DataContainer())
            {
                var dbitems = db.ActivityViews.ToArray();
                //db.Activities.DefaultIfEmpty()
                //.Join(db.Projects,
                //x => x.activityProjectId,
                //x => x.prId,
                //(x, y) => new { Activity = x, Project = y })
                //.Join(db.Users.Where(x => !(x.userIsAnonymous ?? false)),
                //x => x.Activity.activityUserId,
                //x => x.userId,
                //(x, y) => new { Activity = x.Activity, Project = x.Project, User = y })
                //.Join(db.Users.Where(x=>!(x.userIsAnonymous ?? false)),
                //x => x.Activity.activityRelatedUser,
                //x => x.userId,
                //(x, y) => new { Activity = x.Activity, Project = x.Project, User = x.User, RelatedUser = y })
                //.Join(db.Announcements,
                //x => x.Activity.activityAnnouncementId,
                //x => x.annId,
                //(x, y) => new { Activity = x.Activity, Project = x.Project, User = x.User, RelatedUser = x.RelatedUser, Announcement = y })
                ////.Join(db.Comments.DefaultIfEmpty(),
                ////x => x.Activity.activityCommentId,
                ////x => x.cId,
                ////(x, y) => new { Activity = x.Activity, Project = x.Project, User = x.User, RelatedUser = x.RelatedUser, Comment = y })
                //.OrderByDescending(x=>x.Activity.activityDatetime)
                //.Take(15)
                //.ToArray();
                //.Where(x => x.Activity.activityAnnouncementId != null).ToArray();

                var projects = db.Projects.ToArray();
                var users = db.Users.ToArray();

                foreach (var item in items)
                {
                    var record = dbitems.Where(x => x.activityId == item.activityId).FirstOrDefault();

                    if (record == null)
                        continue;

                    item.activityContent = item.activityContent
                        .Replace("{activityUserId}", record.UserId == 0 ? "" : users.Where(x => x.userId == record.UserId).Select(x => x.userName).FirstOrDefault())
                        .Replace("{activityProjectId}", record.ProjectId == 0 ? "" : projects.Where(x => x.prId == record.ProjectId).Select(x => x.prTitle).FirstOrDefault())
                        .Replace("{activityRelatedUser}", record.RelatedUserId == 0 ? "" : users.Where(x => x.userId == record.RelatedUserId).Select(x => x.userName).FirstOrDefault());

                    yield return item;
                }

                db.Database.Connection.Close();
            }
        }

    }
}
