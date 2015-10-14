using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace OrganizationalIdentity.Authorization
{
    public static class AuthorizationExtensions
    {
        public static bool IsInRole(this IPrincipal principal, string role, string organizationId)
        {
            var claimsPrincipal = principal as ClaimsPrincipal;
            return claimsPrincipal?.HasClaim(p => p.Type.Contains(organizationId) && p.Value == role) ?? false;
        }
    }
}
