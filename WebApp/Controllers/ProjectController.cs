using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApp.Base;
using WebApp.Models;
using WebApp.Utilities;

namespace WebApp.Controllers
{
    public class ProjectController : BaseController
    {
        //
        // GET: /Project/
        [HttpGet]
        public ActionResult Index(int id)
        {
            using (var db = new DataContainer())
            {
                var project = db.Projects.Where(x => x.prId == id).FirstOrDefault();

                if (!Utilities.Helpers.IsLoggedIn(HttpContext))
                {
                    return RedirectToAction("Index", "Home");
                }

                if (project == null)
                {
                    return RedirectToAction("Index", "List");
                }

                var model = new Project();
                model.Comments = db.Comments.Where(x => x.cProjectId == id).Select(x => new CommentDetail { Comment = x, Author = x.User }).OrderByDescending(x => x.Comment.cInsertedDate).ToArray();
                model.Details = new ProjectDetails
                {
                    Project = project,
                    Students = project.UserProjects.Where(g => g.Users.userGroupId == 3).Select(g => g.Users).ToArray(),
                    Supervisors = project.UserProjects.Where(g => g.Users.userGroupId == 2 || g.Users.userGroupId == 1).Select(g => g.Users).ToArray()
                };

                model.Files = project.FileLookups.Select(x => new FileLookupDetail { Path = x.fileLookUpPath, User = x.User.userName, Datetime = x.fileLookUpDatetime ?? DateTime.Now }).ToArray();

                return View(model);

            }
        }

        [HttpPost]
        public ActionResult AddComment()
        {
            string commentContent = Request.Form["newcomment"];
            string prid = Request.Form["project"];
            var uid = HttpContext.Session["UserId"];
            var userid = 0;
            var projectid = 0;

            if (HttpContext.Session["UserId"] == null || !Int32.TryParse(HttpContext.Session["UserId"].ToString(), out userid) || !Int32.TryParse(prid, out projectid) || Request.Form["newcomment"] == null)
            {
                return new JsonResult { Data = new { success = false, message = "Invalid Comment" } };
            }

            using (var db = new DataContainer())
            {
                var comment = new Comments();
                comment.cContent = commentContent;
                comment.cInsertedDate = DateTime.Now;
                comment.cUpdateDate = null;
                comment.cUserId = userid;
                comment.cProjectId = projectid;
                comment.cIsActive = true;

                try
                {
                    db.Comments.Add(comment);
                    db.SaveChanges();
                    db.Database.Connection.Close();
                    Activities.AddActivity(userid, projectid, null, comment.cId, LanguageId);
                    return new JsonResult { Data = new { success = true, message = "" } };
                }
                catch
                {
                    return new JsonResult { Data = new { success = false, message = "An error Occurred" } };
                }
            }
        }

        [HttpPost]
        public ActionResult AjaxUpload()
        {
            HttpPostedFileBase file = Request.Files["report"];
            string prid = Request.Form["project"];
            var uid = HttpContext.Session["UserId"];
            var userid = 0;
            var projectid = 0;

            if (HttpContext.Session["UserId"] == null || !Int32.TryParse(HttpContext.Session["UserId"].ToString(), out userid) || !Int32.TryParse(prid, out projectid) || file == null)
            {
                return new JsonResult { Data = new { success = false, message = "Invalid File" } };
            }

            if (!(file.FileName.EndsWith(".doc") || file.FileName.EndsWith(".txt") || file.FileName.EndsWith(".docx") || file.FileName.EndsWith(".ppt") || file.FileName.EndsWith(".pdf") || file.FileName.EndsWith(".rtf") || file.FileName.EndsWith(".xls") || file.FileName.EndsWith(".xlsx")))
            {
                return new JsonResult { Data = new { success = false, message = "Invalid File" } };
            }

            var reportName = DateTime.Now.ToString("yyyyMMddyy") + file.FileName;

            if (file.ContentLength > 0)
            {
                using (var db = new DataContainer())
                {
                    var relativePath = "/Reports/" + reportName;
                    string filePath = HttpContext.Server.MapPath(relativePath);
                    file.SaveAs(filePath);

                    var user = db.Users.Where(x => x.userId == userid).FirstOrDefault();
                    var project = db.Projects.Where(x => x.prId == projectid).FirstOrDefault();

                    var comment = new Comments
                    {
                        cContent = string.Format("Ο χρήστης {0} ανήρτησε μία αναφορά", user.userName),
                        User = user,
                        cInsertedDate = DateTime.Now,
                        cIsActive = true,
                        cUpdateDate = null,
                        Project = project
                    };

                    db.Comments.Add(comment);

                    var lookup = new FileLookup
                    {
                        Project = project,
                        User = user,
                        fileLookUpPath = relativePath,
                        fileLookUpDatetime = DateTime.Now
                    };

                    db.FileLookups.Add(lookup);
                    db.SaveChanges();

                    var emails = project.UserProjects.Select(x => x.Users.userEmail).ToArray();

                    Activities.AddActivity(userid, projectid, null, comment.cId, LanguageId);
                    var body = "A document submited, path http://demo.katheni.com/{file}";

                    foreach (var email in emails)
                    {
                        Utilities.EmailSender.SendEmail("filippis.chris@gmail.com", email, "A document submited", "NetLab", body.Replace("{file}", relativePath));
                    }

                    db.Database.Connection.Close();
                    return new JsonResult { Data = new { success = true, message = "" } };
                }

            }

            return new JsonResult { Data = new { success = false, message = "An error Occurred" } };
        }
    }
}
