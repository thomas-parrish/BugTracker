using System.Collections.Generic;
using BugTracker.Libraries.Comparers;
using BugTracker.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using OrganizationalIdentity.UserManager;

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
            var userManager = new ApplicationUserManager(new OrganizationUserStore<ApplicationUser>(context));
            var roleManager = new RoleManager<OrganizationRole,string>(new RoleStore<OrganizationRole, string, OrganizationUserRole>(context));

            var roles = new List<OrganizationRole>
            {
                new OrganizationRole() {Name = "Submitter"},
                new OrganizationRole() {Name = "Developer"},
                new OrganizationRole() {Name = "Project Manager"},
                new OrganizationRole() {Name = "Admin"},
                new OrganizationRole() {Name = "Divine Being"}
            };

            foreach(var role in roles.Except(context.Roles, new RoleEqualityComparer()))
            {
                roleManager.Create(role);
            }

        }
    }
}
