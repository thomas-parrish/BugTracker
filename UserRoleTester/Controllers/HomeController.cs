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
using OrganizationalIdentity.Authorization;

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

        [OrganizationAuthorize(Roles="Admin", OrganizationProperty = "organizationId")]
        [Route("~/About/{organizationId}")]
        public ActionResult About(string organizationId)
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        [OrganizationAuthorize(Roles="SuperAdmin")]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}