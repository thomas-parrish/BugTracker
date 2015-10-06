using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace OrganizationalIdentity.UserManager
{
    public class OrganizationUserRole : IdentityUserRole<string>
    {
        public string OrganizationId { get; set; }

        public virtual IdentityRole Role { get; set; }
        public virtual Organization Organization { get; set; }
    }
}
