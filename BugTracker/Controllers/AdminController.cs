using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Owin;
using Microsoft.Owin;
using BugTracker.Models;
using BugTracker.Models.Extensions;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;

namespace BugTracker.Controllers
{
    [RoutePrefix("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private ApplicationDbContext _db = new ApplicationDbContext();

        // GET: Admin
        public ActionResult Index()
        {   
            return View(_db.Roles.ToList());
        }

        [Route("Manage/{name}s")]
        public ActionResult Manage(string name)
        {
            if(string.IsNullOrWhiteSpace(name))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            ViewBag.AssignedUsers = new MultiSelectList(_db.Users, "Id", "UserName", 
                                        _db.UsersInRole(name).Select(u => u.Id));
            ViewBag.Name = name;
            return View();
        }

        [Route("Manage/{name}s")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Manage(string name, string[] assignedUsers)
        {
            var manager = HttpContext.GetOwinContext().Get<ApplicationUserManager>();

            assignedUsers = assignedUsers ?? new string[0];

            var role = _db.Roles.FirstOrDefault(r => r.Name == name);

            if(role == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            role.Users.Clear();
            foreach (var userId in assignedUsers)
            {
                manager.AddToRole(userId, name);
            }

            return RedirectToAction("Manage", new {name});
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _db != null)
            {
                _db.Dispose();
                _db = null;
            }

            base.Dispose(disposing);
        }
    }
}