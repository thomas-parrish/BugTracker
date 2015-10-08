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
    public class OrganizationUserRole : OrganizationUserRole<string>
    {

    }

    public class OrganizationUserRole<TKey> : IdentityUserRole<TKey> 
        where TKey : IEquatable<TKey>
    {
        public int Id { get; set; }
        public TKey OrganizationId { get; set; }

        public virtual OrganizationUser<TKey> User { get; set; }
        public virtual OrganizationRole<TKey> Role { get; set; }
        public virtual Organization<TKey> Organization { get; set; }
    }
}
