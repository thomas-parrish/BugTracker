using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrganizationalIdentity.UserManager;

namespace OrganizationalIdentity.Interfaces
{
    public interface IOrganizationStore<in TUser>
       : IDisposable
        where TUser : OrganizationUser
    {
        Task CreateAsync(Organization organization);
        Task UpdateAsync(Organization organization);
        Task DeleteAsync(Organization organization);
        Task DeleteAsync(string id);

        Task AddUser(Organization organization, TUser user);
        Task RemoveUser(Organization organization, TUser user);
    }
}
