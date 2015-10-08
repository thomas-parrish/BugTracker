using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using OrganizationalIdentity.UserManager;
using UserRoleTester.Models;

namespace UserRoleTester.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<UserRoleTester.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(UserRoleTester.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
            var db = new ApplicationDbContext();;

            var m = new OrganizationUserManager<ApplicationUser>(new OrganizationUserStore<ApplicationUser>(db));

            var r = new RoleManager<OrganizationRole>(new RoleStore<OrganizationRole, string, OrganizationUserRole<string>>(db));

            if (!db.Users.Any(u => u.UserName == "tparrish@coderfoundry.com"))
            {
                m.Create(new ApplicationUser()
                {
                    Email = "tparrish@coderfoundry.com",
                    UserName = "tparrish@coderfoundry.com"
                }, "Abc123!");
            }

            if (!db.Organizations.Any(o => o.Name == "Coder Foundry"))
            {
                db.Organizations.Add(new Organization()
                {
                    Name = "Coder Foundry",
                    Description = "Triad's premier .Net coding bootcamp"
                });
                db.SaveChanges();
            }

            var cf = db.Organizations.FirstOrDefault(o => o.Name == "Coder Foundry");
            var tp = db.Users.First(u => u.UserName == "tparrish@coderfoundry.com");
            cf?.Users.Add(tp);

            if (!r.Roles.Any(role => role.Name == "SuperAdmin"))
            {
                r.Create(new OrganizationRole()
                {
                    Name = "SuperAdmin"
                });
            }

            if (!r.Roles.Any(role => role.Name == "Admin"))
            {
                r.Create(new OrganizationRole()
                {
                    Name = "Admin"
                });
            }

            

        }
    }
}
