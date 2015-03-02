using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using WebApp.Base;
using WebApp.Models;
using WebApp.Pocos;
using WebApp.Utilities;

namespace WebApp.Controllers
{
    public class AdministrationController : BaseController
    {
        //
        // GET: /Administration/

        #region General

        public ActionResult Index()
        {
            var userId = 0;

            if (!Utilities.Helpers.IsAdminLoggedIn(HttpContext, out userId))
            {
                return RedirectToAction("Index", "List");
            }

            using (var db = new DataContainer())
            {
                var projects = db.Projects.ToArray();
                db.Database.Connection.Close();
                return View(projects);
            }
        }

        public ActionResult Users()
        {
            using (var db = new DataContainer())
            {
                var users = db.Users.Where(x => !(x.userIsAnonymous ?? false)).ToArray();
                db.Database.Connection.Close();
                return PartialView("PartialViews/Users", users);
            }
        }

        public ActionResult Projects()
        {
            using (var db = new DataContainer())
            {
                var projects = db.Projects.ToArray();
                db.Database.Connection.Close();
                return PartialView("PartialViews/Projects", projects);
            }
        }

        public ActionResult Announcements()
        {
            using (var db = new DataContainer())
            {
                var announcements = db.Announcements.OrderByDescending(x => x.annPublishedDate).ToArray();
                db.Database.Connection.Close();
                return PartialView("PartialViews/Announcements", announcements);
            }
        }

        public ActionResult Events()
        {
            using (var db = new DataContainer())
            {
                var events = db.Events.OrderByDescending(x => x.EventInsertedDate).ToArray();
                db.Database.Connection.Close();
                return PartialView("PartialViews/Events", events);
            }
        }

        public ActionResult TextConstants(int pg = 0)
        {
            using (var db = new DataContainer())
            {
                var textConstants = db.TextConstants.OrderBy(x => x.TextConstName).ToArray();

                var ids = textConstants.Select(x => x.TextConstName).ToArray();

                //new Data.CacheProvider(HttpContext.Cache).Add("test", ids, typeof(WebApp.TextConstants));

                db.Database.Connection.Close();

                ViewBag.Page = pg;
                var o = (double)((double)textConstants.Length / (double)25);
                ViewBag.Pages = Math.Ceiling((double)((double)textConstants.Length / (double)25));

                return PartialView("PartialViews/TextConstants", textConstants.Skip(25 * pg).Take(25).ToArray());
            }
        }

        #endregion

        #region TextConstants

        public ActionResult EditTextConstant(int id)
        {
            using (var db = new DataContainer())
            {
                var textConstant = db.TextConstants
                    .Where(x => x.TextConstId == id)
                    .FirstOrDefault();

                var model = new TextConstant { Datetime = textConstant.TextConstDatetime, Name = textConstant.TextConstName, Id = textConstant.TextConstId, TextConstantValues = textConstant.TextConstantValues.ToArray() };

                db.Database.Connection.Close();
                return View("PartialViews/TextConstants/EditTextConstant", model);
            }
        }

        [HttpPost]
        public JsonResult EditTextConstant(FormCollection form, int id)
        {
            var response = new AjaxResponse() { errors = new List<Error>() { } };
            if (string.IsNullOrWhiteSpace(form["grvalue"]))
            {
                response.errors.Add(new Error { error = "Invalid_Greek_Value", field = "grvalue" });
            }

            if (string.IsNullOrWhiteSpace(form["envalue"]))
            {
                response.errors.Add(new Error { error = "Invalid_Greek_Value", field = "envalue" });
            }
            if (response.errors.Any())
            {
                return new JsonResult { Data = response };
            }
            using (var db = new DataContainer())
            {
                var textConstants = db.TextConstantValues.Where(x => x.tConstValConstId == id).ToArray();

                if (textConstants.Any(x => x.tConstValLangId == 1))
                {
                    var textConstant = textConstants.Where(x => x.tConstValLangId == 1).FirstOrDefault();
                    textConstant.tConstValValue = form["grvalue"].ToString();
                }

                if (textConstants.Any(x => x.tConstValLangId == 2))
                {
                    var textConstant = textConstants.Where(x => x.tConstValLangId == 2).FirstOrDefault();
                    textConstant.tConstValValue = form["envalue"].ToString();
                }

                Strings.ExpireTextConstants();
                db.SaveChanges();
                db.Database.Connection.Close();
            }

            response.success = true;
            response.message = "Ενημερώθηκε επιτυχώς";
            return new JsonResult { Data = response };
        }

        public JsonResult DeleteTextConstant(int id)
        {
            var userId = 0;

            var response = new AjaxResponse() { errors = new List<Error>() { } };

            if (Utilities.Helpers.IsAdminLoggedIn(HttpContext, out userId))
            {
                using (var db = new DataContainer())
                {
                    var textConstant = db.TextConstants.Where(x => x.TextConstId == id).FirstOrDefault();

                    foreach (var textConstantValue in textConstant.TextConstantValues.ToArray())
                    {
                        db.TextConstantValues.Remove(textConstantValue);
                    }

                    db.TextConstants.Remove(textConstant);
                    db.SaveChanges();
                    db.Database.Connection.Close();
                    response.success = true;
                    return new JsonResult { Data = response, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
            }

            response.reload = true;
            response.message = "Η ανακοίνωση διαγράφηκε επιτυχώς";
            return new JsonResult { Data = response, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        }

        #endregion

        #region User Controllers

        //[OutputCache(Duration = 86400, VaryByParam = "none")]
        public ActionResult AddUser()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            using (var db = new DataContainer())
            {
                db.UserGroups.ToList().ForEach(x =>
                {
                    items.Add(new SelectListItem { Text = x.userGroupName, Value = x.userGroupId.ToString() });
                });
            }

            ViewBag.Group = items;

            return PartialView("PartialViews/User/AddUser");

        }

        [HttpPost]
        public JsonResult AddUser(FormCollection form)
        {
            var response = new AjaxResponse { errors = new List<Error>() { } };

            if (!Utilities.Validation.IsValidMail(form["Email"]))
            {
                response.errors.Add(new Error { error = "Παρακαλώ συμπληρώστε σωστό email", field = "Email" });
            }

            if (string.IsNullOrWhiteSpace(form["FirstName"]))
            {
                response.errors.Add(new Error { error = "Παρακαλώ συμπληρώστε όνομα", field = "FirstName" });
            }

            if (string.IsNullOrWhiteSpace(form["LastName"]))
            {
                response.errors.Add(new Error { error = "Παρακαλώ συμπληρώστε επίθετο", field = "LastName" });
            }

            int groupid = 0;
            if (form["Group"] == null || !int.TryParse(form["Group"].ToString(), out groupid))
            {
                response.errors.Add(new Error { error = "Παρακαλώ επιλέξτε σωστό UserGroup", field = "Group" });
            }

            if (response.errors.Count() == 0)
            {
                using (var db = new DataContainer())
                {
                    var email = form["Email"];
                    var active = form["Active"].Contains("1");
                    var userName = form["Username"];
                    var existsEmail = db.Users.Any(x => x.userEmail == email);
                    var existsUsername = db.Users.Any(x => x.userName == userName);

                    if (existsEmail)
                    {
                        response.errors.Add(new Error { error = "Ο email χρησιμοποιείται", field = "Email" });
                        response.success = false;
                    }

                    if (existsUsername)
                    {
                        response.errors.Add(new Error { error = "Ο username χρησιμοποιείται", field = "Username" });
                        response.success = false;
                    }

                    if (response.errors.Count() != 0)
                    {
                        db.Database.Connection.Close();
                        return new JsonResult { Data = response };
                    }

                    var hash = Utilities.Helpers.GetHashedPassword("user");
                    var pass = hash[1];
                    var salt = hash[0];

                    var user = new Users
                    {
                        userGroupId = groupid,
                        userEmail = email,
                        userLastVisitDate = DateTime.Now,
                        userName = userName,
                        userPassword = pass,
                        userRegistrationDate = DateTime.Now,
                        userSalt = salt,
                        userIsActive = active,
                        userIsAnonymous = false,
                        userIsNew = true,
                        userLastName = form["LastName"],
                        userFirstName = form["FirstName"],
                        userImage = "",
                    };

                    db.Users.Add(user);

                    db.SaveChanges();

                    db.Database.Connection.Close();

                    Utilities.EmailSender.SendEmail("filippis.chris@gmail.com", user.userEmail, "Registration Email", "NetLab", "Μόλις ΄΄εγινε μέλος στο netlab χρησιμοποίησε το mail σου και τον κωδικό \"user\" για να συνδεθείς.");

                    response.message = "Ο χρήστης μπορεί να συνδεθει με το email που δοθηκε και με κωδικό την λέξη \"user\"και στην συνέχεια να αλλάξει τον κωδικό του";
                    response.success = !response.errors.Any();
                }
            }

            return new JsonResult { Data = response };
        }

        public JsonResult DeleteUser(int id)
        {
            var userId = 0;

            if (Utilities.Helpers.IsAdminLoggedIn(HttpContext, out userId))
            {
                using (var db = new DataContainer())
                {
                    var user = db.Users.Where(x => x.userId == id).FirstOrDefault();

                    if (user == null)
                    {
                        db.Database.Connection.Close();
                        return new JsonResult { Data = new { success = false, message = "Invalid User to Delete" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                    }

                    var userProjects = db.UserProjects.Where(x => x.upUserId == user.userId).ToArray();

                    foreach (var item in userProjects)
                    {
                        db.UserProjects.Remove(item);
                        //anonymous userId
                        item.upUserId = 34;
                    }

                    var userComments = db.Comments.Where(x => x.cUserId == user.userId).ToArray();

                    foreach (var item in userComments)
                    {
                        //anonymous userId
                        item.cUserId = 34;
                    }

                    db.SaveChanges();

                    db.Users.Remove(user);

                    db.SaveChanges();

                    return new JsonResult { Data = new { success = true }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
            }

            return new JsonResult { Data = new { success = false, message = "Invalid User" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public ActionResult EditUser(int id)
        {
            List<SelectListItem> items = new List<SelectListItem>();

            var user = new Users();
            using (var db = new DataContainer())
            {
                user = db.Users.Where(x => x.userId == id).FirstOrDefault();
                db.UserGroups.ToList().ForEach(x =>
                {
                    items.Add(new SelectListItem { Text = x.userGroupName, Selected = x.userGroupId == user.userGroupId, Value = x.userGroupId.ToString() });
                });
            }

            ViewBag.Group = items;

            return View("PartialViews/User/EditUserNew", user);
        }

        [HttpPost]
        public JsonResult EditUser(FormCollection form, int id)
        {
            if (string.IsNullOrWhiteSpace(form["userEmail"]) || !Utilities.Validation.IsValidMail(form["userEmail"].ToString()))
            {
                return new JsonResult { Data = new { Success = false, Message = "Invalid Email" } };
            }

            if (string.IsNullOrWhiteSpace(form["userFirstName"]))
            {
                return new JsonResult { Data = new { Success = false, Message = "Invalid FirstName" } };
            }

            if (string.IsNullOrWhiteSpace(form["userLastName"]))
            {
                return new JsonResult { Data = new { Success = false, Message = "Invalid LastName" } };
            }

            using (var db = new DataContainer())
            {
                var User = db.Users.Where(x => x.userId == id).FirstOrDefault();
                User.userEmail = form["userEmail"].ToString();
                User.userFirstName = form["userFirstName"].ToString();
                User.userLastName = form["userLastName"].ToString();
                User.userGroupId = Int32.Parse(form["Group"].ToString());
                User.userIsActive = form["userIsActive"].Contains("1");

                db.SaveChanges();
                db.Database.Connection.Close();
            }

            return new JsonResult { Data = new { success = true, message = "ok" } };
        }

        public ActionResult ChangePassword(int id)
        {
            using (var db = new DataContainer())
            {
                var userId = db.Users.Where(x => x.userId == id).Select(x => x.userId).FirstOrDefault();
                db.Database.Connection.Close();
                return View("PartialViews/User/ChangePassword", userId);
            }

        }

        [HttpPost]
        public JsonResult ChangePassword(FormCollection form, int id)
        {
            using (var db = new DataContainer())
            {
                var user = db.Users.Where(x => x.userId == id).FirstOrDefault();
                if (user != null)
                {
                    var formPass = form["password"];
                    var useFormPass = !string.IsNullOrWhiteSpace(formPass);

                    var newPass = useFormPass ? formPass : Utilities.Helpers.GetRandomPassword();
                    var hashArray = Utilities.Helpers.GetHashedPassword(newPass);

                    var salt = hashArray[0];
                    var password = hashArray[1];

                    user.userSalt = salt;
                    user.userPassword = password;
                    if (!useFormPass)
                    {
                        var emailSent = Utilities.EmailSender.SendEmail("", user.userEmail, "NewPass", "Name", string.Format("new pass {0}", newPass));
                        if (emailSent)
                        {
                            db.SaveChanges();
                            db.Database.Connection.Close();
                            return new JsonResult { Data = new { Success = true, Message = "Έχει σταλεί με email ο καινούριος κωδικός" } };
                        }
                    }

                    db.SaveChanges();
                    db.Database.Connection.Close();
                    return new JsonResult { Data = new { Success = true, Message = "Ο κωδικός άλλαξε επιτυχώς" } };

                }

                db.Database.Connection.Close();
                return new JsonResult { Data = new { Success = false, Message = "Invalid User" } };

            }

        }

        #endregion

        #region Project Controllers

        public ActionResult ProjectDetails(int id)
        {
            var userId = 0;

            if (Utilities.Helpers.IsAdminLoggedIn(HttpContext, out userId))
            {
                using (var db = new DataContainer())
                {
                    var project = db.Projects.FirstOrDefault(x => x.prId == id);

                    return PartialView("", project);
                }
            }

            return new JsonResult { Data = new { success = false, message = "Invalid User" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public JsonResult ProjectDetails(Projects project)
        {
            var userId = 0;

            if (Utilities.Helpers.IsAdminLoggedIn(HttpContext, out userId))
            {
                using (var db = new DataContainer())
                {


                    return new JsonResult { Data = new { success = true }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
            }

            return new JsonResult { Data = new { success = false, message = "Invalid User" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        //[OutputCache(Duration = 86400, VaryByParam = "none")]
        public ActionResult AddProject()
        {
            var Types = new List<SelectListItem>() { };
            using (var db = new DataContainer())
            {
                db.ProjectTypes.ToList().ForEach(x =>
                {
                    Types.Add(new SelectListItem { Text = x.typeTitle, Value = x.typeId.ToString() });
                });

                ViewBag.Type = Types;
                ViewBag.Categories = db.ProjectCategories.ToArray().Select(x => new SelectListItem { Text = x.catTitle, Value = x.catId.ToString() }).ToList();
            }

            return PartialView("PartialViews/Project/AddProject");
        }

        [HttpPost]
        public JsonResult AddProject(FormCollection form)
        {
            var response = new AjaxResponse { errors = new List<Error>() { } };

            if (string.IsNullOrWhiteSpace(form["Title"]))
            {
                response.errors.Add(new Error { error = "Παρακαλώ συμπληρώστε τίτλο", field = "Title" });
            }

            if (string.IsNullOrWhiteSpace(form["Description"]))
            {
                response.errors.Add(new Error { error = "Παρακαλώ συμπληρώστε περιγραφή", field = "Description" });
            }

            if (string.IsNullOrWhiteSpace(form["Subject"]))
            {
                response.errors.Add(new Error { error = "Παρακαλώ συμπληρώστε θέμα", field = "Subject" });
            }

            var formats = new[] { "dd/MM/yyyy", "dd/MM/yy", "dd-MM-yyyy" };
            var startDate = new DateTime();
            if (!DateTime.TryParseExact(form["StartDate"], formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out startDate))
            {
                response.errors.Add(new Error { error = "Παρακαλώ συμπληρώστε σωστή ημερομηνία", field = "StartDate" });
            }

            var endDate = new DateTime();
            if (!DateTime.TryParseExact(form["EndDate"], formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out endDate))
            {
                response.errors.Add(new Error { error = "Παρακαλώ συμπληρώστε σωστή ημερομηνία", field = "EndDate" });
            }


            if (response.errors.Count() == 0)
            {
                using (var db = new DataContainer())
                {
                    var project = new Projects
                    {
                        prDescription = form["Description"],
                        prTitle = form["Title"],
                        prInsertDate = DateTime.Now,
                        prUpdateDate = DateTime.Now,
                        prTypeId = Convert.ToInt32(form["Type"]),
                        prCatId = Convert.ToInt32(form["Categories"]),
                        prKeywords = form["Keywords"],
                        prStartDate = startDate,
                        prEndDate = endDate,
                        prSubject = form["Subject"],
                        prExtraInfo = form["Info"],
                        prIsPaper = (form["IsPaper"] ?? "") == "on",
                    };

                    db.Projects.Add(project);

                    db.SaveChanges();
                    db.Database.Connection.Close();
                    Activities.AddProject(Int32.Parse(Session["UserId"].ToString()), project.prId, LanguageId);
                    response.message = "Το Project με τίτλο " + project.prTitle + " προστέθηκε";
                    response.success = response.errors.Count() == 0;
                }
            }

            return new JsonResult { Data = response };
        }

        public ActionResult EditProject(int id)
        {
            var project = new ProjectDetails { };

            using (var db = new DataContainer())
            {
                project = db.Projects.Where(x => x.prId == id)
                    .Select(x =>
                        new
                        {
                            Project = x,
                            Students = x.UserProjects.Select(g => g.Users).Where(g => g.userGroupId == 3),
                            Supervisors = x.UserProjects.Select(g => g.Users).Where(g => g.userGroupId != 3)
                        }).ToArray().Select(x => new ProjectDetails
                        {
                            Project = x.Project,
                            Students = x.Students.ToArray(),
                            Supervisors = x.Supervisors.ToArray()
                        }).FirstOrDefault();

                List<SelectListItem> students = new List<SelectListItem>();
                List<SelectListItem> supervisors = new List<SelectListItem>();

                var supervisorsids = project.Supervisors.Select(g => g.userId).ToArray();
                var studentsids = project.Students.Select(g => g.userId).ToArray();
                db.Users.Where(x => !studentsids.Contains(x.userId) && !supervisorsids.Contains(x.userId) && !(x.userIsAnonymous ?? false) && x.userIsActive).ToList().ForEach(x =>
                {
                    if (x.userGroupId == 3 || x.userGroupId == 2)
                    {
                        students.Add(new SelectListItem { Text = x.userName, Value = x.userId.ToString() });
                    }

                    if (x.userGroupId == 1 || x.userGroupId == 2)
                    {
                        supervisors.Add(new SelectListItem { Text = x.userName, Value = x.userId.ToString() });
                    }
                });

                ViewBag.Students = students;
                ViewBag.Supervisors = supervisors;
                ViewBag.Categories = db.ProjectCategories.ToArray().Select(x => new SelectListItem { Text = x.catTitle, Value = x.catId.ToString(), Selected = x.catId == project.Project.prCatId }).ToList();
                ViewBag.Type = db.ProjectTypes.ToArray().Select(x => new SelectListItem { Text = x.typeTitle, Value = x.typeId.ToString(), Selected = x.typeId == project.Project.prTypeId }).ToList();

                db.Database.Connection.Close();
            }

            return View("PartialViews/Project/EditProject", project);
        }

        [HttpPost]
        public JsonResult EditProject(FormCollection form, int id)
        {
            var response = new AjaxResponse { errors = new List<Error>() { } };
            var title = form["Title"];
            var description = form["Description"];
            if (string.IsNullOrWhiteSpace(title))
            {
                response.errors.Add(new Error { error = "Παρακαλώ συμπληρώστε σωστό τίτλο", field = "Title" });
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                response.errors.Add(new Error { error = "Παρακαλώ συμπληρώστε σωστή περιγραφή", field = "Description" });
            }

            if (string.IsNullOrWhiteSpace(form["Subject"]))
            {
                response.errors.Add(new Error { error = "Παρακαλώ συμπληρώστε θέμα", field = "Subject" });
            }

            var formats = new[] { "dd/MM/yyyy", "dd/MM/yy", "dd-MM-yyyy", "dd-MM-yy" };
            var startDate = new DateTime();
            if (!DateTime.TryParseExact(form["StartDate"], formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out startDate))
            {
                response.errors.Add(new Error { error = "Παρακαλώ συμπληρώστε σωστή ημερομηνία", field = "StartDate" });
            }

            var endDate = new DateTime();
            if (!DateTime.TryParseExact(form["EndDate"], formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out endDate))
            {
                response.errors.Add(new Error { error = "Παρακαλώ συμπληρώστε σωστή ημερομηνία " + form["EndDate"], field = "EndDate" });
            }

            if (response.errors.Any()) 
            {
                return new JsonResult { Data = response };
            }

            using (var db = new DataContainer())
            {
                var Project = db.Projects.Where(x => x.prId == id).FirstOrDefault();
                Project.prTitle = title;
                Project.prDescription = description;
                Project.prStartDate = startDate;
                Project.prEndDate = endDate;
                Project.prTypeId = Convert.ToInt32(form["Type"]);
                Project.prCatId = Convert.ToInt32(form["Categories"]);
                Project.prKeywords = form["Keywords"];
                Project.prSubject = form["Subject"];
                Project.prExtraInfo = form["Info"];
                Project.prIsPaper = (form["IsPaper"] ?? "") == "on";

                db.SaveChanges();
                db.Database.Connection.Close();

                response.message = "Η εγγραφή ενημερώθηκε επιτυχώς";
                response.success = response.errors.Count() == 0;

            }

            return new JsonResult { Data = response };
        }

        public JsonResult DeleteProject(int id)
        {
            var userId = 0;

            if (Utilities.Helpers.IsAdminLoggedIn(HttpContext, out userId))
            {
                using (var db = new DataContainer())
                {
                    var project = db.Projects.Where(x => x.prId == id).FirstOrDefault();

                    if (project == null)
                    {
                        db.Database.Connection.Close();
                        return new JsonResult { Data = new { success = false, message = "Invalid Project" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                    }

                    var userProjects = db.UserProjects.Where(x => x.upProjectId == project.prId).ToArray();

                    foreach (var item in userProjects)
                    {
                        db.UserProjects.Remove(item);
                    }


                    var projectComments = db.Comments.Where(x => x.cProjectId == project.prId).ToArray();

                    foreach (var item in projectComments)
                    {
                        db.Comments.Remove(item);
                    }

                    db.Projects.Remove(project);
                    Activities.RemoveProject(userId, id, LanguageId);
                    db.SaveChanges();

                    return new JsonResult { Data = new { success = true }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
            }

            return new JsonResult { Data = new { success = false, message = "Invalid User" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public JsonResult AddProjectUser(FormCollection form, int id)
        {
            var userid = 0;
            if (string.IsNullOrWhiteSpace(form["uid"]) || !Int32.TryParse(form["uid"], out userid))
            {
                return new JsonResult { Data = new { } };
            }

            using (var db = new DataContainer())
            {
                var project = db.Projects.Where(x => x.prId == id).FirstOrDefault();

                var userProject = new UserProjects
                {
                    upProjectId = id,
                    upUserId = userid
                };
                db.UserProjects.Add(userProject);
                db.SaveChanges();
                db.Database.Connection.Close();
                Activities.AddProjectUser(Int32.Parse(Session["UserId"].ToString()), id, userid, LanguageId);
            }

            return new JsonResult { Data = new { } };

        }

        [HttpPost]
        public JsonResult RemoveProjectUser(FormCollection form, int id)
        {
            var userid = 0;
            if (string.IsNullOrWhiteSpace(form["uid"]) || !Int32.TryParse(form["uid"], out userid))
            {
                return new JsonResult { Data = new { Success = false, Message = "Invalid User Id" } };
            }

            using (var db = new DataContainer())
            {
                var Project = db.Projects.Where(x => x.prId == id).FirstOrDefault();
                var RecordToDelete = Project.UserProjects.Where(x => x.Users.userId == userid).FirstOrDefault();

                db.UserProjects.Remove(RecordToDelete);

                db.SaveChanges();
                Activities.RemoveProjectUsers(Int32.Parse(Session["UserId"].ToString()), id, userid, LanguageId);
                db.Database.Connection.Close();
            }

            return new JsonResult { Data = new { success = true, message = "ok" } };
        }

        #endregion

        #region Announcements

        //[OutputCache(Duration = 86400, VaryByParam = "none")]
        public ActionResult AddAnnouncement()
        {
            return PartialView("PartialViews/Announcement/AddAnnouncement");
        }

        [HttpPost]
        public JsonResult AddAnnouncement(FormCollection form)
        {
            var content = form["Content"];
            var title = form["Title"];
            var response = new AjaxResponse() { errors = new List<Error>() { } };
            var userId = 0;

            if (Utilities.Helpers.IsAdminLoggedIn(HttpContext, out userId))
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    response.errors.Add(new Error { error = "Empty Content", field = "Content" });
                }

                if (string.IsNullOrWhiteSpace(title))
                {
                    response.errors.Add(new Error { error = "Empty Title", field = "Title" });
                }

                if (response.errors.Any())
                {
                    return new JsonResult { Data = response };
                }

                using (var db = new DataContainer())
                {
                    var announcement = new Announcements
                    {
                        annContent = content,
                        annPublishedDate = DateTime.Now,
                        annUserId = userId,
                        annTitle = title,
                    };

                    db.Announcements.Add(announcement);
                    db.SaveChanges();
                    db.Database.Connection.Close();

                    Activities.AddAnnouncement(userId, announcement.annId, LanguageId);

                    response.success = true;
                    response.message = "Η ανακοίνωση προστέθηκε επιτυχώς";

                    return new JsonResult { Data = response };
                }
            }

            return new JsonResult { Data = new AjaxResponse { success = false, reload = true } };
        }

        public ActionResult EditAnnouncement(int id)
        {
            var userId = 0;

            Utilities.Helpers.IsAdminLoggedIn(HttpContext, out userId);

            using (var db = new DataContainer())
            {
                var announcement = db.Announcements.Where(x => x.annId == id && userId != 0).FirstOrDefault();
                return PartialView("PartialViews/Announcement/AddAnnouncement", announcement);
            }
        }

        [HttpPost]
        public JsonResult EditAnnouncement(FormCollection form, int id)
        {
            var content = form["Content"];
            var title = form["Title"];
            var response = new AjaxResponse() { errors = new List<Error>() { } };
            var userId = 0;

            if (Utilities.Helpers.IsAdminLoggedIn(HttpContext, out userId))
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    response.errors.Add(new Error { error = "Empty Content", field = "Content" });
                }

                if (string.IsNullOrWhiteSpace(title))
                {
                    response.errors.Add(new Error { error = "Empty Title", field = "Title" });
                }

                if (response.errors.Any())
                {
                    return new JsonResult { Data = response };
                }

                using (var db = new DataContainer())
                {
                    var announcement = db.Announcements.Where(x => x.annId == id).FirstOrDefault();

                    if (announcement == null)
                    {
                        return new JsonResult { Data = new AjaxResponse { success = false, reload = true } };
                    }

                    announcement.annContent = content;
                    announcement.annTitle = title;

                    db.SaveChanges();

                    response.success = true;
                    response.message = "Οι αλλαγές αποθηκεύτηκαν επιτυχώς";

                    db.Database.Connection.Close();

                    return new JsonResult { Data = response };
                }
            }

            return new JsonResult { Data = new AjaxResponse { success = false, reload = true } };
        }

        public JsonResult DeleteAnnouncement(int id)
        {
            var userId = 0;

            var response = new AjaxResponse() { errors = new List<Error>() { } };

            if (Utilities.Helpers.IsAdminLoggedIn(HttpContext, out userId))
            {
                using (var db = new DataContainer())
                {
                    var announcement = db.Announcements.Where(x => x.annId == id).FirstOrDefault();
                    db.Announcements.Remove(announcement);
                    db.SaveChanges();
                    db.Database.Connection.Close();
                    response.success = true;
                    return new JsonResult { Data = response, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
            }

            response.reload = true;
            response.message = "Η ανακοίνωση διαγράφηκε επιτυχώς";
            return new JsonResult { Data = response, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        }

        #endregion

        #region Event Controllers

        //[OutputCache(Duration = 86400, VaryByParam = "none")]
        public ActionResult AddEvent()
        {
            using (var db = new DataContainer())
            {
                ViewBag.Project = db.Projects.ToArray().Select(x => new SelectListItem { Text = x.prTitle, Value = x.prId.ToString() }).ToArray();
                db.Database.Connection.Close();
                return PartialView("PartialViews/Event/AddEvent");
            }
        }

        [HttpPost]
        public JsonResult AddEvent(FormCollection form)
        {
            var response = new AjaxResponse { errors = new List<Error>() { } };

            var title = form["Title"];
            var description = form["Description"];
            var Datetime = new DateTime(1900, 1, 1);
            var project = 0;
            var dat = form["Datetime"];

            int.TryParse(form["Project"], out project);

            if (string.IsNullOrWhiteSpace(title))
            {
                response.errors.Add(new Error { error = "Παρακαλώ συμπληρώστε σωστό τίτλο", field = "Title" });
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                response.errors.Add(new Error { error = "Παρακαλώ συμπληρώστε σωστή περιγραφή", field = "Description" });
            }

            var formats = new[] { "dd/MM/yyyy", "dd/MM/yy", "dd-MM-yyyy" };
            if (!DateTime.TryParseExact(dat, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out Datetime))
            {
                response.errors.Add(new Error { error = "Παρακαλώ συμπληρώστε σωστή περιγραφή", field = "Datetime" });
            }

            if (response.errors.Count() == 0)
            {
                using (var db = new DataContainer())
                {
                    title = title.RemoveDiacritics();
                    if (db.Events.Any(x => x.EventFriendlyUrl == title))
                    {
                        db.Database.Connection.Close();
                        response.errors.Add(new Error { error = "Παρακαλώ συμπληρώστε σωστό τίτλο", field = "Title" });
                    }

                    var Event = new Events
                    {
                        EventDescription = description,
                        EventTitle = title,
                        EventLocation = form["Location"],
                        EventInsertedDate = DateTime.Now,
                        EventUpdateDate = DateTime.Now,
                        EventProjectId = project,
                        EventDate = Datetime,
                        EventFriendlyUrl = title.Replace(" ","_")
                    };

                    db.Events.Add(Event);

                    db.SaveChanges();
                    db.Database.Connection.Close();
                    response.message = "Το Event με τίτλο " + Event.EventTitle + " προστέθηκε";
                    response.success = response.errors.Count() == 0;
                }
            }

            return new JsonResult { Data = response };
        }

        public ActionResult EditEvent(int id)
        {
            using (var db = new DataContainer())
            {
                var Event = db.Events.Where(x => x.EventId == id).FirstOrDefault();
                ViewBag.Project = db.Projects.ToArray().Select(x => new SelectListItem { Text = x.prTitle, Value = x.prId.ToString(), Selected = Event.EventProjectId == x.prId }).ToArray();

                db.Database.Connection.Close();
                return View("PartialViews/Event/EditEvent", Event);
            }
        }

        [HttpPost]
        public JsonResult EditEvent(FormCollection form, int id)
        {
            var response = new AjaxResponse { errors = new List<Error>() { } };
            var title = form["Title"];
            var description = form["Description"];
            var Datetime = new DateTime(1900, 1, 1);
            var project = 0;

            int.TryParse(form["Project"], out project);
            DateTime.TryParse(form["Datetime"], out Datetime);

            if (string.IsNullOrWhiteSpace(title))
            {
                response.errors.Add(new Error { error = "Παρακαλώ συμπληρώστε σωστό τίτλο", field = "Title" });
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                response.errors.Add(new Error { error = "Παρακαλώ συμπληρώστε σωστή περιγραφή", field = "Description" });
            }

            if (Datetime == new DateTime(1900, 1, 1))
            {
                response.errors.Add(new Error { error = "Παρακαλώ συμπληρώστε σωστή περιγραφή", field = "Datetime" });
            }

            using (var db = new DataContainer())
            {
                var Event = db.Events.Where(x => x.EventId == id).FirstOrDefault();
                if (!db.Projects.Any(x => x.prId == project))
                {
                    response.errors.Add(new Error { error = "Παρακαλώ συμπληρώστε σωστή περιγραφή", field = "Project" });
                }

                Event.EventTitle = title;
                Event.EventDescription = description;
                Event.EventProjectId = project;
                Event.EventLocation = form["Location"];
                Event.EventDate = Datetime;
                Event.EventFriendlyUrl = title.Replace(" ", "_");

                db.SaveChanges();
                db.Database.Connection.Close();

                response.message = Strings.GetTextConstant("Successfull_update", LanguageId, "Η εγγραφή ενημερώθηκε επιτυχώς");
                response.success = response.errors.Count() == 0;

            }

            return new JsonResult { Data = response };
        }

        public JsonResult DeleteEvent(int id)
        {
            var userId = 0;

            if (Utilities.Helpers.IsAdminLoggedIn(HttpContext, out userId))
            {
                using (var db = new DataContainer())
                {
                    var Event = db.Events.Where(x => x.EventId == id).FirstOrDefault();

                    if (Event == null)
                    {
                        db.Database.Connection.Close();
                        return new JsonResult { Data = new { success = false, message = "Invalid Event" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                    }
                    db.Events.Remove(Event);
                    db.SaveChanges();

                    return new JsonResult { Data = new { success = true }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
            }

            return new JsonResult { Data = new { success = false, message = "Invalid User" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        #endregion

        [NonAction]
        private void RemoveCache(string actionName, string controllerName, object routeData)
        {
            var url = Url.Action(actionName, controllerName, routeData);
            //var ff = HttpContext.Cache.AsQueryable();
            Response.RemoveOutputCacheItem(Url.Action(actionName, controllerName, routeData));
        }
    }
}
