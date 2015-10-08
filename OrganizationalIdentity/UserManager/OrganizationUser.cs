using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;

namespace OrganizationalIdentity.UserManager
{
    public class OrganizationUser : OrganizationUser<string>
    {
        public OrganizationUser() : base()
        {
            this.Id = Guid.NewGuid().ToString();
        }
    }

    public class OrganizationUser<TKey> : IdentityUser<TKey, IdentityUserLogin<TKey>, OrganizationUserRole<TKey>, IdentityUserClaim<TKey>>
        where TKey : IEquatable<TKey>
    {
        public OrganizationUser()
        {
            Organizations = new HashSet<Organization<TKey>>();
        }
        public virtual ICollection<Organization<TKey>> Organizations { get; }
    }
}
