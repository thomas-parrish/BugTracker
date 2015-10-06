using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using BugTracker.Libraries.Helpers;
using BugTracker.Models;
using Octokit;
using OrganizationalIdentity.Models;

namespace BugTracker.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var githubApi = new GitHubClient(new ProductHeaderValue("dePested"))
                {
                    Credentials = new Credentials(User.Identity.GetGithubAccessToken())
                };
                var t = await githubApi.Organization.GetAllForCurrent();
                var m = t.Count();

                var u = User.Identity as ClaimsIdentity;
                var q = u.Claims.Where(c => c.ValueType.Contains("Json")).Select(n => new JsonClaim(n));
            }
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }
    }
}