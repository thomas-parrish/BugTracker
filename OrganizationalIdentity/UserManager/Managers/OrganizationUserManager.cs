using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer.Utilities;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;

namespace OrganizationalIdentity.UserManager
{

    public class OrganizationUserManager<TUser> : UserManager<TUser, string> 
        where TUser: OrganizationUser
    {
        public OrganizationUserManager(OrganizationUserStore<TUser> store)
            : base(store)
        {

        }

        public override async Task<IdentityResult> AddClaimAsync(string userId, Claim claim)
        {
            var result = await base.AddClaimAsync(userId, claim);

            if (result == IdentityResult.Success)
                TriggerRegenerateIdentity(userId);
            return result;
        }

        public override async Task<IdentityResult> AddToRoleAsync(string userId, string role)
        {
            var result = await base.AddToRoleAsync(userId, role);
            if (result == IdentityResult.Success)
                TriggerRegenerateIdentity(userId);
            return result;
        }
        public async Task<IdentityResult> AddToRoleAsync(string userId, string organizationId, string role)
        {
            var userRoleStore = Store as OrganizationUserStore<TUser>;
            if( userRoleStore == null )
                throw new InvalidCastException("Could not convert Store to OrganizationUserStore");
            var user = await FindByIdAsync(userId).WithCurrentCulture();
            if (user == null)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, "User with id {0} was not found.",
                    userId));
            }
            var userRoles = await userRoleStore.GetRolesAsync(user).WithCurrentCulture();
            if (userRoles.Contains(role))
            {
                return new IdentityResult($"User is already in the role {role}.");
            }
            await userRoleStore.AddToRoleAsync(user, organizationId, role).WithCurrentCulture();
            TriggerRegenerateIdentity(userId);
            return await UpdateAsync(user).WithCurrentCulture();
        }

        public void AddToRole(string userId, string organizationId, string roleName)
        {
            AsyncHelper.RunSync(() => AddToRoleAsync(userId, organizationId, roleName));
        }

        public override async Task<IdentityResult> AddToRolesAsync(string userId, params string[] roles)
        {
            var result = await base.AddToRolesAsync(userId, roles);
            if (result == IdentityResult.Success)
                TriggerRegenerateIdentity(userId);
            return result;
        }

        public override async Task<IdentityResult> RemoveClaimAsync(string userId, Claim claim)
        {
            var result = await base.RemoveClaimAsync(userId, claim);
            if (result == IdentityResult.Success)
                TriggerRegenerateIdentity(userId);
            return result;
        }

        public override async Task<IdentityResult> RemoveFromRoleAsync(string userId, string role)
        {
            var result = await base.RemoveFromRoleAsync(userId, role);
            if (result == IdentityResult.Success)
                TriggerRegenerateIdentity(userId);
            return result;
        }

        public override async Task<IdentityResult> RemoveFromRolesAsync(string userId, params string[] roles)
        {
            var result = await base.RemoveFromRolesAsync(userId, roles);
            if (result == IdentityResult.Success)
                TriggerRegenerateIdentity(userId);
            return result;
        }

        public override async Task<ClaimsIdentity> CreateIdentityAsync(TUser user, string authenticationType)
        {
            var identity = await base.CreateIdentityAsync(user, authenticationType);
            //Add custom claims here
            var store = Store as OrganizationUserStore<TUser>;
            if(store == null)
                throw new InvalidCastException("Store is not an OrganizationUserStore");

            var organizationRoles = await store.GetOrganizationRolesAsync(user);

            if(organizationRoles!= null)
                foreach(var role in organizationRoles)
                    identity.AddClaim(new Claim($"OrganizationRole/{role.OrganizationId}", role.Role.Name));

            return identity;
        }

        public bool TriggerRegenerateIdentity(string userId)
        {
            var user = Users.FirstOrDefault(u => u.Id.Equals(userId));
            if (user == null) return false;

            HttpRuntime.Cache.Add(user.UserName,
                1,
                null,
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.High,
                null);
            return true;
        }
    }
}
