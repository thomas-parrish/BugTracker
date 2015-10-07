using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using OrganizationalIdentity.UserManager;
using UserRoleTester.Models;

namespace UserRoleTester.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var db = new ApplicationDbContext(); 

            var m = new OrganizationUserManager<ApplicationUser>(new OrganizationUserStore<ApplicationUser>(db));

            var r = new RoleManager<OrganizationRole, string>(new RoleStore<OrganizationRole, string, OrganizationUserRole>(db));

            if (!db.Users.Any(u => u.UserName == "tparrish@coderfoundry.com"))
            {
                m.Create(new ApplicationUser()
                {
                    Email = "tparrish@coderfoundry.com",
                    UserName = "tparrish@coderfoundry.com"
                }, "Abc123!");
            }

            if (!db.Organizations.Any(o => o.Name == "Coder Foundry"))
            {
                db.Organizations.Add(new Organization()
                {
                    Name = "Coder Foundry",
                    Description = "Triad's premier .Net coding bootcamp"
                });
                db.SaveChanges();
            }

            var cf = db.Organizations.FirstOrDefault(o => o.Name == "Coder Foundry");
            var tp = db.Users.First(u => u.UserName == "tparrish@coderfoundry.com");
            if (!cf.Users.Contains(tp))
                cf.Users.Add(tp);

            if (!r.Roles.Any(role => role.Name == "SuperAdmin"))
            {
                r.Create(new OrganizationRole()
                {
                    Name = "SuperAdmin"
                });
            }

            if (!r.Roles.Any(role => role.Name == "Admin"))
            {
                r.Create(new OrganizationRole()
                {
                    Name = "Admin"
                });
            }

            m.AddToRole(tp, cf.Id, "Admin");
            m.AddToRole(tp.Id, "SuperAdmin");

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}