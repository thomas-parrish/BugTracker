using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using BugTracker.Libraries.Helpers;
using Octokit;

namespace BugTracker.Controllers
{
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
            }
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }
    }
}