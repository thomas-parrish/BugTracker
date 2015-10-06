using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using BugTracker.Models;

namespace BugTracker
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

    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
            
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context) 
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.
            manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "Your security code is {0}"
            });
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is {0}"
            });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = 
                    new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
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

    // Configure the application sign-in manager which is used in this application.
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override async Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            var externalIdentity = await AuthenticationManager.GetExternalIdentityAsync(DefaultAuthenticationTypes.ExternalCookie);

            var localIdentity = await user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);

            foreach (var item in externalIdentity.Claims)
            {
                if (!localIdentity.HasClaim(o => o.Type == item.Type))
                    localIdentity.AddClaim(item);
            }

            return localIdentity;
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }
}
