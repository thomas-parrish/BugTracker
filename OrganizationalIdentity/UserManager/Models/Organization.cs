using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrganizationalIdentity.UserManager
{
    public class Organization
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual ICollection<OrganizationUser> Users { get; set; } = new HashSet<OrganizationUser>();
        public virtual ICollection<OrganizationUserRole> Roles { get; set; } = new HashSet<OrganizationUserRole>();

        public Organization() : base()
        {
            Id = Guid.NewGuid().ToString();
        }

    } 
}
