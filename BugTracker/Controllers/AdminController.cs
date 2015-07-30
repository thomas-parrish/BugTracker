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
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private ApplicationDbContext Db = new ApplicationDbContext();

        // GET: Admin
        public ActionResult Index()
        {   
            return View(Db.Roles.ToList());
        }

        public ActionResult Add(string name)
        {
            if(string.IsNullOrWhiteSpace(name))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            ViewBag.AssignedUsers = new MultiSelectList(Db.UsersNotInRole(name), "Id", "UserName");
            ViewBag.Name = name;
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Add(string name, string[] AssignedUsers)
        {
            var manager = HttpContext.GetOwinContext().Get<ApplicationUserManager>();
            foreach (var userId in AssignedUsers)
            {
                manager.AddToRole(userId, name);
            }

            return RedirectToAction("Index");
        }

        public ActionResult Remove(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            ViewBag.AssignedUsers = new MultiSelectList(Db.UsersInRole(name), "Id", "UserName");
            ViewBag.Name = name;
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Remove(string name, string[] AssignedUsers)
        {
            var manager = HttpContext.GetOwinContext().Get<ApplicationUserManager>();
            foreach (var userId in AssignedUsers)
            {
                manager.RemoveFromRole(userId, name);
            }

            return RedirectToAction("Index");
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing && Db != null)
            {
                Db.Dispose();
                Db = null;
            }

            base.Dispose(disposing);
        }
    }
}