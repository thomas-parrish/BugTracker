using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrganizationalIdentity.UserManager
{
    public class OrganizationManager<TUser>
        where TUser: OrganizationUser
    {
        private OrganizationStore<TUser> Store { get; set; }

        public OrganizationManager(OrganizationStore<TUser> store)
        {
            Store = store;
        }

        public static OrganizationManager<TUser> Create(OrganizationDbContext<TUser> context)
        {
            return new OrganizationManager<TUser>(new OrganizationStore<TUser>(context));
        } 
    }
}
