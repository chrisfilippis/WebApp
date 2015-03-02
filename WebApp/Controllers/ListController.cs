using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApp.Base;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class ListController : BaseController
    {
        //
        // GET: /List/
        [ActionName("Index")]
        [HttpGet]
        public ActionResult Index()
        {
            var userId = 0;

            if (!Utilities.Helpers.IsLoggedIn(HttpContext, out userId))
            {
                return RedirectToAction("Index", "Home");
            }

            using (var db = new DataContainer())
            {
                var projects = db.UserProjects.Where(x => x.upUserId == userId).Select(x => x.Projects).ToList();

                var projectdetails = projects.OrderByDescending(x => x.prUpdateDate).Select(x => new ProjectDetails { Project = x, Students = x.UserProjects.Where(g => g.Users.userGroupId == 3).Select(g => g.Users).ToArray(), Supervisors = x.UserProjects.Where(g => g.Users.userGroupId == 2 || g.Users.userGroupId == 1).Select(g => g.Users).ToArray() }).ToArray();

                var model = new ProjectList();
                model.User = db.Users.Where(x => x.userId == userId).FirstOrDefault();
                model.Projects = projectdetails;

                return View(model);

            }
        }
    }
}
