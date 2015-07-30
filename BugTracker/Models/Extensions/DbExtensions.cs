using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using BugTracker.Libraries.Comparers;

namespace BugTracker.Models.Extensions
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
    }
}