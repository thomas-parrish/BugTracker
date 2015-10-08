using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.Annotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;

namespace OrganizationalIdentity.UserManager
{
    public class OrganizationDbContext<TUser> : OrganizationDbContext<TUser, string> where TUser : OrganizationUser<string>
    {
        public OrganizationDbContext() : base()
        {
        }

        public OrganizationDbContext(DbCompiledModel model) : base(model)
        {
        }

        public OrganizationDbContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
        }

        public OrganizationDbContext(string nameOrConnectionString, DbCompiledModel model)
            : base(nameOrConnectionString, model)
        {
        }

        public OrganizationDbContext(DbConnection existingConnection, bool contextOwnsConnection)
            : base(existingConnection, contextOwnsConnection)
        {
        }

        public OrganizationDbContext(DbConnection existingConnection, DbCompiledModel model, bool contextOwnsConnection)
            : base(existingConnection, model, contextOwnsConnection)
        {
        }
    }

    public class OrganizationDbContext<TUser, TKey> :
        IdentityDbContext
            <TUser, OrganizationRole<TKey>, TKey, IdentityUserLogin<TKey>,
                OrganizationUserRole<TKey>, IdentityUserClaim<TKey>> 
        where TUser: OrganizationUser<TKey>
        where TKey : struct, IEquatable<TKey>
    {
        public OrganizationDbContext() : base()
        {
        }

        public OrganizationDbContext(DbCompiledModel model) : base(model)
        {
        }

        public OrganizationDbContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
        }

        public OrganizationDbContext(string nameOrConnectionString, DbCompiledModel model)
            : base(nameOrConnectionString, model)
        {
        }

        public OrganizationDbContext(DbConnection existingConnection, bool contextOwnsConnection)
            : base(existingConnection, contextOwnsConnection)
        {
        }

        public OrganizationDbContext(DbConnection existingConnection, DbCompiledModel model, bool contextOwnsConnection)
            : base(existingConnection, model, contextOwnsConnection)
        {
        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException("modelBuilder");
            }

            var org = modelBuilder.Entity<Organization>()
                .ToTable("AspNetOrganizations");
            org.HasMany(o => o.Users).WithMany(u => u.Organizations).Map(map =>
            {
                map.MapLeftKey("OrganizationId");
                map.MapRightKey("UserId");
                map.ToTable("AspNetOrganizationUsers");
            });
            org.HasMany(o => o.Roles).WithOptional().HasForeignKey(r => r.OrganizationId);


            // Needed to ensure subclasses share the same table
            var user = modelBuilder.Entity<OrganizationUser>()
                .ToTable("AspNetUsers");
            user.HasMany(u => u.Roles).WithRequired().HasForeignKey(ur => ur.UserId);
            user.HasMany(u => u.Claims).WithRequired().HasForeignKey(uc => uc.UserId);
            user.HasMany(u => u.Logins).WithRequired().HasForeignKey(ul => ul.UserId);
            user.Property(u => u.UserName)
                .IsRequired()
                .HasMaxLength(256)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("UserNameIndex") { IsUnique = true }));

            // CONSIDER: u.Email is Required if set on options?
            user.Property(u => u.Email).HasMaxLength(256);

            var ourModel = modelBuilder.Entity<OrganizationUserRole<TKey>>();
            ourModel
                .ToTable("AspNetOrganizationUserRoles")
                .HasKey(r => r.Id);
            ourModel
                .HasOptional(m => m.Organization)
                .WithMany(o=>o.Roles)
                .HasForeignKey(k => k.OrganizationId);
            ourModel
                .Property(u=>u.UserId)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Unique_AspNetOrganizationUserRoles") { IsUnique = true, Order = 0}));
            ourModel
                .Property(u => u.RoleId)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Unique_AspNetOrganizationUserRoles") { IsUnique = true, Order = 1 }));
            ourModel
                .Property(u => u.OrganizationId)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Unique_AspNetOrganizationUserRoles") { IsUnique = true, Order = 2 }));
            
            modelBuilder.Entity<IdentityUserLogin>()
                .HasKey(l => new { l.LoginProvider, l.ProviderKey, l.UserId })
                .ToTable("AspNetUserLogins");

            modelBuilder.Entity<IdentityUserClaim>()
                .ToTable("AspNetUserClaims");

            //DOES THIS NEED TO BE CHANGED?
            var role = modelBuilder.Entity<OrganizationRole>()
                .ToTable("AspNetRoles");
            role.HasKey(r => r.Id);
            role.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(256)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("RoleNameIndex") { IsUnique = true }));
            role.HasMany(r => r.Users).WithRequired().HasForeignKey(ur => ur.RoleId);
        }

        public IDbSet<OrganizationUserRole<TKey>> UserRoles { get; set; }
        public IDbSet<Organization<TKey>> Organizations { get; set; }  
    }
}
