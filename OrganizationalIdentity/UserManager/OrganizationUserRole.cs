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
        public int Id { get; set; }
        public string OrganizationId { get; set; }
        
        //Not sure what to do here, to allow easy extension of the user class
        //with custom properties.
        public virtual OrganizationUser User { get; set; }
        public virtual OrganizationRole Role { get; set; }
        public virtual Organization Organization { get; set; }
    }
}
