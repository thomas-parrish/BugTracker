using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using BugTracker.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;

namespace BugTracker.Libraries.Helpers
{
    public static class GithubHelpers
    {
        public static async Task<ApplicationUser> CreateUserFromIdentityAsync(this ApplicationUserManager manager, ExternalLoginInfo externalLogin)
        {
            var user = new ApplicationUser
            {
                UserName = externalLogin.DefaultUserName,
                Email = externalLogin.Email
            };

            foreach(var claim in externalLogin.ExternalIdentity.Claims.Where(c=>c.Type.StartsWith("urn:github:")))
                user.Claims.Add(new IdentityUserClaim() {ClaimType = claim.Type, ClaimValue = claim.Value, UserId = user.Id});

            var result = await manager.CreateAsync(user);
            return result.Succeeded ? user : null;
        }

        public static async Task UpdateClaimsFromExternalIdentityAsync(this ApplicationUserManager manager, ApplicationUser user, ExternalLoginInfo externalLogin)
        {
            foreach (var claim in user.Claims
                                    .Where(c=> c.ClaimType.StartsWith("urn:github"))
                                    .Select(c => new Claim(c.ClaimType, c.ClaimValue))
                                    .ToList())
            {
                await manager.RemoveClaimAsync(user.Id, claim);
            }

            foreach (var claim in externalLogin.ExternalIdentity.Claims
                                    .Where(c => c.Type.StartsWith("urn:github:"))
                                    .Select(c=>new Claim(c.Type, c.Value))
                                    .ToList())
            {
                await manager.AddClaimAsync(user.Id, claim);
            }
        }

        public static string GetGithubAccessToken(this IIdentity user)
        {
            return ((ClaimsIdentity)user).Claims.FirstOrDefault(c => c.Type == "urn:github:access_token")?.Value;
        }

        public static string GetGithubName(this IIdentity user)
        {
            return ((ClaimsIdentity)user).Claims.FirstOrDefault(c => c.Type == "urn:github:name")?.Value;
        }

        public static string GetGithubAvatar(this IIdentity user)
        {
            return ((ClaimsIdentity)user).Claims.FirstOrDefault(c => c.Type == "urn:github:avatar_url")?.Value;
        }
    }
}