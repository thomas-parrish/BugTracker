using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace OrganizationalIdentity.UserManager
{
    public class OrganizationUserStore<TUser> :
        UserStore<TUser, OrganizationRole, string, IdentityUserLogin, OrganizationUserRole, IdentityUserClaim>,
        IUserStore<TUser> where TUser : OrganizationUser
    {
        public OrganizationUserStore(DbContext context) : base(context)

        {
            
        } 
    }
}
