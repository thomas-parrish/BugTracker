using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;

namespace OrganizationalIdentity.UserManager
{
    public class OrganizationRole : IdentityRole<string, OrganizationUserRole<string>>
    {
        public OrganizationRole() 
        {
            Id = Guid.NewGuid().ToString();
        }


    }

    public class OrganizationRole<TKey> : IdentityRole<TKey,OrganizationUserRole<TKey>> 
        where TKey : IEquatable<TKey>
    {
        public OrganizationRole() { }

        public OrganizationRole(string roleName) : this()
        {
            Name = roleName;
        }
    }
}
