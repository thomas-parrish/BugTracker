using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace OrganizationalIdentity.Authorization
{
    public class OrganizationAuthorizeAttribute : AuthorizeAttribute
    {

        public string OrganizationProperty { get; set; }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var authorized = base.AuthorizeCore(httpContext);

            if (string.IsNullOrWhiteSpace(OrganizationProperty))
                return authorized;

            var organizationId = (string) (httpContext.Request.RequestContext.RouteData.Values[OrganizationProperty] ??
                                              httpContext.Request.Params.Get(OrganizationProperty) ?? 
                                              httpContext.Request.QueryString.Get(OrganizationProperty) ??
                                              httpContext.Request.Form.Get(OrganizationProperty));
            if(string.IsNullOrWhiteSpace(organizationId))
                throw new ArgumentException("The OrganizationId could not be extracted from the OrganizationProperty");
            return Roles.Split(',').Any(role => httpContext.User.IsInRole(role, organizationId));
        }
    }
}
