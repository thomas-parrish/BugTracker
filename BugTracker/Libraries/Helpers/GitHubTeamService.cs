using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Octokit;

namespace BugTracker.Libraries.Helpers
{
    public class GitHubTeamService
    {
        private GitHubClient Client { get; set; }

        public GitHubTeamService(string clientToken)
        {
            Client = new GitHubClient(new ProductHeaderValue("dePested"))
            {
                Credentials = new Credentials(clientToken)
            };
        }

        public async Task<List<Models.Team>> GetRemoteTeams()
        {
            var teams = (await Client.Organization.GetAllForCurrent())
                .Select(t => new Models.Team()
                {
                    Name = t.Name ?? t.Login,
                    GitHubId = t.Id
                });
            return teams.ToList();
        }
    }
}