using System.Collections.Generic;
using Microsoft.AspNet.Identity.EntityFramework;
using OrganizationalIdentity.UserManager;

namespace BugTracker.Libraries.Comparers
{
    class RoleEqualityComparer : IEqualityComparer<OrganizationRole>
    {
        public bool Equals(OrganizationRole x, OrganizationRole y)
        {
            return x.Name == y.Name;
        }

        public int GetHashCode(OrganizationRole obj)
        {
            unchecked
            {
                if (obj == null)
                    return 0;
                int hashCode = obj.Name.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.Name.GetHashCode();
                return hashCode;
            }
        }
    }
}