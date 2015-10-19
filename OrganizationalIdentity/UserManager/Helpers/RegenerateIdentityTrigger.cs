using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security.Cookies;

namespace OrganizationalIdentity.UserManager
{
    public class RegenerateIdentityTrigger<TUserManager, TUser>
        where TUserManager : OrganizationUserManager<TUser> 
        where TUser : OrganizationUser
    {
        public static async Task Validate(CookieValidateIdentityContext context)
        {
            // invalidate user cookie if user's security stamp have changed
            var invalidateBySecurityStamp = SecurityStampValidator.OnValidateIdentity<TUserManager, TUser>(
                    validateInterval: TimeSpan.FromMinutes(30),
                    regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager));
            await invalidateBySecurityStamp.Invoke(context);

            if (context.Identity == null || !context.Identity.IsAuthenticated)
            {
                return;
            }
            if (HttpRuntime.Cache[context.Identity.Name] != null)
            {
                // get user manager. It must be registered with OWIN
                var userManager = context.OwinContext.GetUserManager<TUserManager>();
                var username = context.Identity.Name;

                // get new user identity with updated properties
                var updatedUser = await userManager.FindByNameAsync(username);

                // updated identity from the new data in the user object
                var newIdentity = await updatedUser.GenerateUserIdentityAsync(userManager);

                // kill old cookie
                context.OwinContext.Authentication.SignOut(context.Options.AuthenticationType);

                // sign in again
                var authenticationProperties = new AuthenticationProperties() { IsPersistent = context.Properties.IsPersistent };
                context.OwinContext.Authentication.SignIn(authenticationProperties, newIdentity);

                context.ReplaceIdentity(newIdentity);
                HttpRuntime.Cache.Remove(context.Identity.Name);
            }
        }
    }
}
