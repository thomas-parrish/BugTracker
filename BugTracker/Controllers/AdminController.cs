using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using BugTracker.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace BugTracker.Controllers
{
    [RoutePrefix("Admin")]
    [Authorize]
    public class AdminController : Controller
    {
        private ApplicationDbContext _db = new ApplicationDbContext();

        // GET: Admin
        public ActionResult Index()
        {
            var t = User.IsInRole("Admin");
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
        public async Task<ActionResult> Manage(string name, string[] assignedUsers)
        {
            var manager = HttpContext.GetOwinContext().Get<ApplicationUserManager>();
            assignedUsers = assignedUsers ?? new string[0];

            foreach (var user in _db.Users.Where(u => !assignedUsers.Contains(u.Id)))
            {
                await manager.RemoveFromRoleAsync(user.Id, name);
            }
            foreach (var userId in assignedUsers)
            {
                await manager.AddToRoleAsync(userId, name);
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