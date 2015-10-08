using System;
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
    public class OrganizationUserManager<TUser> : OrganizationUserManager<TUser, string>
        where TUser : OrganizationUser
    {
        public OrganizationUserManager(OrganizationUserStore<TUser> store)
            : base(store)
        {

        }
    }

    public class OrganizationUserManager<TUser, TKey> : UserManager<TUser, TKey> 
        where TUser: OrganizationUser<TKey>
        where TKey: IEquatable<TKey>
    {
        public OrganizationUserManager(OrganizationUserStore<TUser, TKey> store)
            : base(store)
        {

        }
        public override async Task<IdentityResult> AddClaimAsync(TKey userId, Claim claim)
        {
            var result = await base.AddClaimAsync(userId, claim);

            if (result == IdentityResult.Success)
                TriggerRegenerateIdentity(userId);
            return result;
        }

        public override async Task<IdentityResult> AddToRoleAsync(TKey userId, string role)
        {
            var result = await base.AddToRoleAsync(userId, role);
            if (result == IdentityResult.Success)
                TriggerRegenerateIdentity(userId);
            return result;
        }
        public async Task<IdentityResult> AddToRoleAsync(TUser user, TKey organizationId, string role)
        {
            var orgStore = Store as OrganizationUserStore<TUser, TKey>;
            if(orgStore == null)
                throw new InvalidCastException("Cannot convert UserStore to OrganizationUserStore");

            await orgStore.AddToRoleAsync(user, organizationId, role);
            TriggerRegenerateIdentity(user.Id);
            return IdentityResult.Success;
        }

        public void AddToRole(TUser user, TKey organizationId, string roleName)
        {
            AsyncHelper.RunSync(() => AddToRoleAsync(user, organizationId, roleName));
        }

        public override async Task<IdentityResult> AddToRolesAsync(TKey userId, params string[] roles)
        {
            var result = await base.AddToRolesAsync(userId, roles);
            if (result == IdentityResult.Success)
                TriggerRegenerateIdentity(userId);
            return result;
        }
        

        public override async Task<IdentityResult> RemoveClaimAsync(TKey userId, Claim claim)
        {
            var result = await base.RemoveClaimAsync(userId, claim);
            if (result == IdentityResult.Success)
                TriggerRegenerateIdentity(userId);
            return result;
        }

        public override async Task<IdentityResult> RemoveFromRoleAsync(TKey userId, string role)
        {
            var result = await base.RemoveFromRoleAsync(userId, role);
            if (result == IdentityResult.Success)
                TriggerRegenerateIdentity(userId);
            return result;
        }

        public override async Task<IdentityResult> RemoveFromRolesAsync(TKey userId, params string[] roles)
        {
            var result = await base.RemoveFromRolesAsync(userId, roles);
            if (result == IdentityResult.Success)
                TriggerRegenerateIdentity(userId);
            return result;
        }

        public bool TriggerRegenerateIdentity(TKey userId)
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
