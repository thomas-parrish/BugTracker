using System;
using System.Configuration;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using BugTracker.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Owin.Security.Providers.GitHub;

namespace BugTracker
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context, user manager and signin manager to use a single instance per request
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    OnValidateIdentity = async context =>
                    {
                        // invalidate user cookie if user's security stamp have changed
                        var invalidateBySecurityStamp = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
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
                            var userManager = context.OwinContext.GetUserManager<ApplicationUserManager>();
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
            });            
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Enables the application to remember the second login verification factor such as phone or email.
            // Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
            // This is similar to the RememberMe option when you log in.
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);
            
            var gitHubConfig = new GitHubAuthenticationOptions()
            {
                ClientId = ConfigurationManager.AppSettings["GithubId"],
                ClientSecret = ConfigurationManager.AppSettings["GithubSecret"],
                Provider = new GitHubAuthenticationProvider()
                {
                    OnAuthenticated = (context) =>
                    {
                        context.Identity.AddClaim(new Claim("urn:github:avatar_url", (string)context.User.GetValue("avatar_url")));
                        context.Identity.AddClaim(new Claim("urn:github:access_token", context.AccessToken));
                        return Task.FromResult(0);
                    }
                }
            };

            app.UseGitHubAuthentication(gitHubConfig);
        }
    }
}