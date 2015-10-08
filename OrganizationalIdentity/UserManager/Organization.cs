using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrganizationalIdentity.UserManager
{
    public class Organization : Organization<string>
    {
        public Organization() : base()
        {
            Id = Guid.NewGuid().ToString();
        }

    }

    public class Organization<TKey>
        where TKey : IEquatable<TKey>
    {
        public TKey Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual ICollection<OrganizationUser<TKey>> Users { get; set; }
        public virtual ICollection<OrganizationUserRole<TKey>> Roles { get; set; }

        public Organization()
        {
            Users = new HashSet<OrganizationUser<TKey>>();
            Roles = new HashSet<OrganizationUserRole<TKey>>();
        }
    } 
}
