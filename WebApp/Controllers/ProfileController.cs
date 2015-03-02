using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApp.Base;
using WebApp.Models;
using WebApp.Pocos;

namespace WebApp.Controllers
{
    public class ProfileController : BaseController
    {
        //
        // GET: /Profile/

        public ActionResult Index(int? id)
        {
            if (!Utilities.Helpers.IsLoggedIn(HttpContext))
            {
                return RedirectToAction("Index", "Home");
            }

            var userDetails = RetriveUserData(id ?? 0);

            return View("Index", userDetails);
        }

        [HttpGet]
        public ActionResult ChangeImage()
        {
            //string myString = Utilities.ViewUtilities.RenderViewToString(this.ControllerContext, "~/Views/Profile/ImageForm.chtml", "~/Views/Shared/Site.Master", this.ViewData, this.TempData);
            return PartialView("ImageForm");
        }

        [HttpGet]
        public JsonResult LogOut()
        {
            HttpContext.Session.Clear();
            return new JsonResult { Data = new { success = true }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public JsonResult SaveImage(FormCollection form)
        {
            var userId = HttpContext.Session["UserId"] != null ? int.Parse(HttpContext.Session["UserId"].ToString()) : 0;

            if (Request.Files["image"] == null)
                return new JsonResult { Data = new { success = false } };

            HttpPostedFileBase file = Request.Files["image"];

            if (!(file.FileName.EndsWith(".jpg") || file.FileName.EndsWith(".png") || file.FileName.EndsWith(".jpeg")) || file.ContentLength == 0)
            {
                return new JsonResult { Data = new { success = false, message = "Invalid File" } };
            }

            var reportName = string.Format("{0}_profile_{1}", userId, file.FileName);

            using (var db = new DataContainer())
            {
                var user = db.Users.Where(x => x.userId == userId).FirstOrDefault();

                if (user == null)
                    return new JsonResult { Data = new { success = false } };

                user.userImage = "/Profiles/" + reportName;

                db.SaveChanges();
            }

            string filePath = Path.Combine(HttpContext.Server.MapPath("/Profiles"), reportName);
            file.SaveAs(filePath);

            return new JsonResult { Data = new { success = true, message = "/Profiles" + reportName } };
        }

        [HttpPost]
        public JsonResult Summary(int? id)
        {
            var sessionUserId = HttpContext.Session["UserId"];
            var userid = id == null ? Convert.ToInt32(sessionUserId == null ? "0" : sessionUserId.ToString()) : id;

            if (userid == 0)
            {
                return new JsonResult { Data = new { success = false, text = "userid is empty" } };
            }
            using (var db = new DataContainer())
            {
                var user = db.Users.Where(x => x.userId == userid).FirstOrDefault();

                var text = View("Summary", user);

                return new JsonResult { Data = new { success = true, text = text } };
            }

        }

        [HttpPost]
        public JsonResult ChangePassword(FormCollection form)
        {
            int userid;
            var response = new AjaxResponse { errors = new List<Error>() { } };

            if (Utilities.Helpers.IsLoggedIn(HttpContext, out userid))
            {
                if (string.IsNullOrEmpty(form["oldpass"]))
                {
                    response.errors.Add(new Error { error = "Ο κωδικός δεν είναι σωστός", field = "user-old-pass" });
                }

                if (string.IsNullOrEmpty(form["newpass"]))
                {
                    response.errors.Add(new Error { error = "Παρακαλώ συμπληρώστε κωδικό", field = "user-new-pass" });
                }

                if (string.IsNullOrEmpty(form["confirmpass"]) || form["newpass"] != form["confirmpass"])
                {
                    response.errors.Add(new Error { error = "Ο κωδικός δεν ταιριάζει", field = "user-confirm-pass" });
                }

                if (response.errors.Count() == 0)
                {
                    using (var db = new DataContainer())
                    {
                        var user = db.Users.Where(x => x.userId == userid).FirstOrDefault();

                        if (user != null)
                        {
                            var oldPassHash = Utilities.Helpers.GetHashedPassword(form["oldpass"], user.userSalt);
                            var oldpass = oldPassHash[1];

                            if (user.userPassword == oldpass)
                            {

                                var hashed = Utilities.Helpers.GetHashedPassword(form["newpass"]);
                                var salt = hashed[0];
                                var pass = hashed[1];

                                user.userSalt = salt;
                                user.userPassword = pass;

                                db.SaveChanges();
                                db.Database.Connection.Close();
                                response.message = "Ο κωδικός άλλαξε επιτυχώς";
                                response.success = true;
                            }
                            else
                            {
                                db.Database.Connection.Close();
                                response.errors.Add(new Error { error = "Ο κωδικός δεν είναι σωστός", field = "user-old-pass" });
                            }
                        }

                        else
                        {
                            db.Database.Connection.Close();
                            response.message = "Please login again";
                            response.success = false;
                        }

                    }
                }
            }
            else
            {
                response.message = "Please login again";
                response.success = false;
            }

            response.success = response.success ? response.errors.Count() == 0 : false;

            return new JsonResult { Data = response };
        }

        private ProfileDetails RetriveUserData(int id = 0)
        {
            var userid = 0;

            userid = id;
            if (userid == 0)
            {
                userid = HttpContext.Session["UserId"] == null ? 0 : Convert.ToInt32(HttpContext.Session["UserId"].ToString());
            }

            using (var db = new DataContainer())
            {
                return db.Users.Where(x => x.userId == userid).AsEnumerable().Select(x => new ProfileDetails { User = x, Projects = x.UserProjects.Select(y => y.Projects).ToArray() }).FirstOrDefault();
            }
        }
    }
}
