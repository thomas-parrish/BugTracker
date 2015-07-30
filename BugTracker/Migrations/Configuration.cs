using System.Collections.Generic;
using BugTracker.Libraries.Comparers;
using BugTracker.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace BugTracker.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<BugTracker.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            ContextKey = "BugTracker.Models.ApplicationDbContext";
        }

        protected override void Seed(BugTracker.Models.ApplicationDbContext context)
        {
            var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>());
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>());

            var roles = new List<IdentityRole>
            {
                new IdentityRole() {Name = "Submitter"},
                new IdentityRole() {Name = "Developer"},
                new IdentityRole() {Name = "Project Manager"},
                new IdentityRole() {Name = "Admin"}
            };

            foreach(var role in roles.Except(context.Roles, new RoleEqualityComparer()))
            {
                roleManager.Create(role);
            }
        }
    }
}
