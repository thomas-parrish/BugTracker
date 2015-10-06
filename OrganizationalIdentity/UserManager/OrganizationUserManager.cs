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
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your email service here to send an email.
            return Task.FromResult(0);
        }
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }

    public class OrganizationUserManager<TUser> : UserManager<TUser,string> where TUser: OrganizationUser
    {
        public OrganizationUserManager(IUserStore<TUser, string> store)
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

        public bool TriggerRegenerateIdentity(string userId)
        {
            var user = Users.FirstOrDefault(u => u.Id == userId);
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
