using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using BugTracker.Libraries.Comparers;
using ExpressionFusion;
using Microsoft.Ajax.Utilities;

namespace BugTracker.Models
{
    public static class DbExtensions
    {
        public static IEnumerable<ApplicationUser> UsersInRole(this ApplicationDbContext context, string roleName)
        {
            return (from u in context.Users
                where
                    (from r in u.Roles
                        join role in context.Roles on r.RoleId equals role.Id
                        where role.Name == roleName
                        select 1).Any()
                select u);
        }

        public static IEnumerable<ApplicationUser> UsersNotInRole(this ApplicationDbContext context, string roleName)
        {
            return (from u in context.Users
                    where
                        !(from r in u.Roles
                         join role in context.Roles on r.RoleId equals role.Id
                         where role.Name == roleName
                         select 1).Any()
                    select u);
        }

        public static bool Update<T>(this ApplicationDbContext context, T item, params string[] changedPropertyNames) where T : class, new()
        {
            context.Set<T>().Attach(item);
            foreach (var propertyName in changedPropertyNames)
            {
                // If we can't find the property, this line wil throw an exception, 
                //which is good as we want to know about it
                context.Entry(item).Property(propertyName).IsModified = true;
            }
            return true;
        }

        public static void EnsureExists<T>(this ApplicationDbContext context, ref T item, Expression<Func<T, T, bool>> equals ) where T: class
        {
            var fixArg = equals.Partial().Apply(item).Result;
            var existing = context.Set<T>().FirstOrDefault(fixArg);
            if (existing != null)
                item = existing;
            else
                context.SaveChanges();
        }
    }
}