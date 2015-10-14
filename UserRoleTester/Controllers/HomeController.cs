using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using OrganizationalIdentity.UserManager;
using UserRoleTester.Migrations;
using UserRoleTester.Models;

namespace UserRoleTester.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {
            bool t;
            if (User.IsInRole("Admin"))
                t = User.IsInRole("Admin");

            var db = new ApplicationDbContext(); 
            new Configuration().Seed(db, true);

            var m = new OrganizationUserManager<ApplicationUser>(new OrganizationUserStore<ApplicationUser>(db));

            var u = User as ClaimsPrincipal;

            return View();
        }

        [Authorize(Roles="Admin")]
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        [Authorize(Roles="SuperAdmin")]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}