using System.Collections.Generic;
using Microsoft.AspNet.Identity.EntityFramework;

namespace BugTracker.Libraries.Comparers
{
    class RoleEqualityComparer : IEqualityComparer<IdentityRole>
    {
        public bool Equals(IdentityRole x, IdentityRole y)
        {
            return x.Name == y.Name;
        }

        public int GetHashCode(IdentityRole obj)
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