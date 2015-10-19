using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace OrganizationalIdentity.UserManager
{
    public class OrganizationUser : IdentityUser<string, IdentityUserLogin, OrganizationUserRole, IdentityUserClaim>
    {
        public OrganizationUser() : base()
        {
            this.Id = Guid.NewGuid().ToString();
        }
        public virtual ICollection<Organization> Organizations { get; set; }
    }

    public static class OrganizationUserExtensions
    {
        public static async Task<ClaimsIdentity> GenerateUserIdentityAsync<TUser>(this TUser user, OrganizationUserManager<TUser> manager)
            where TUser : OrganizationUser
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }
}
