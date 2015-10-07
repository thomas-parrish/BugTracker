using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace OrganizationalIdentity.UserManager
{
    public class OrganizationUserStore<TUser> :
        UserStore<TUser, OrganizationRole, string, IdentityUserLogin, OrganizationUserRole, IdentityUserClaim>,
        IUserStore<TUser> where TUser : OrganizationUser
    {
        public OrganizationUserStore(DbContext context) : base(context)
        {
        }

        public async Task AddToRoleAsync(TUser user, string organizationId, string roleName)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (String.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentException("roleName cannot be empty","roleName");
            }
            var roleEntity = await Context.Set<OrganizationRole>().SingleOrDefaultAsync(r => r.Name.ToUpper() == roleName.ToUpper()).WithCurrentCulture();
            if (roleEntity == null)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                    "Role not found", roleName));
            }
            var orgUserEntity = await Context.Set<Organization>().SingleOrDefaultAsync(o => o.Id == organizationId);
            if (orgUserEntity == null)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                    "Organization not found", roleName));
            }
            if (!orgUserEntity.Users.Any(u => u.Id == user.Id))
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                    "The user is not a member of the organization", roleName));
            }

            var ur = new OrganizationUserRole() { UserId = user.Id, RoleId = roleEntity.Id, OrganizationId = organizationId};
            if (!orgUserEntity.Roles.Any(our => our.OrganizationId == organizationId && our.RoleId == roleEntity.Id && our.UserId == user.Id))
            {
                Context.Set<OrganizationUserRole>().Add(ur);
                if (AutoSaveChanges)
                    await Context.SaveChangesAsync();    
            }
        }
    }
}
