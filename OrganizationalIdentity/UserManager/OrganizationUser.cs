using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;

namespace OrganizationalIdentity.UserManager
{
    public class OrganizationUser : IdentityUser<string, IdentityUserLogin, OrganizationUserRole, IdentityUserClaim>
    {
        public OrganizationUser() : base()
        {
            this.Id = Guid.NewGuid().ToString();
        }
        public virtual ICollection<Organization> Organizations { get; }
    }
}
