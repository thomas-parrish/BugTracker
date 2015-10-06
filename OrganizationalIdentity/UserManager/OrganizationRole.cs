using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;

namespace OrganizationalIdentity.UserManager
{
    public class OrganizationRole : IdentityRole<string, OrganizationUserRole>
    {
        public OrganizationRole() : base()
        {
        }

        public string Tickle { get; set; }
    }
}
