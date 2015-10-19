using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace OrganizationalIdentity.Authorization
{
    public enum PropertyLocation
    {
        Route,
        Params,
        Form,
        Session,
        Cookie,
        Query,
        Default
    }

    public class OrganizationAuthorizeAttribute : AuthorizeAttribute
    {
        public string OrganizationProperty { get; set; }
        public string OrganizationRoles { get; set; }
        public PropertyLocation PropertyLocation { get; set; }
        public bool UseSession { get; set; } = false;
        public bool UseCookies { get; set; } = false;
        public string CookieName { get; set; }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var authorized = base.AuthorizeCore(httpContext);

            if (string.IsNullOrWhiteSpace(OrganizationProperty))
                return authorized;

            string organizationId = null;

            switch (PropertyLocation)
            {
                case PropertyLocation.Route:
                    organizationId = (string)httpContext.Request.RequestContext.RouteData.Values[OrganizationProperty];
                    break;
                case PropertyLocation.Params:
                    organizationId = httpContext.Request.Params[OrganizationProperty];
                    break;
                case PropertyLocation.Form:
                    organizationId = httpContext.Request.Form[OrganizationProperty];
                    break;
                case PropertyLocation.Session:
                    organizationId = (string) httpContext.Session?[OrganizationProperty];
                    break;
                case PropertyLocation.Cookie:
                    if (string.IsNullOrWhiteSpace(CookieName))
                        throw new ArgumentNullException(nameof(CookieName), "The cookie name must be provided");
                    organizationId = httpContext.Request.Cookies?[CookieName]?.Values?[OrganizationProperty];
                    break;
                case PropertyLocation.Query:
                    organizationId = httpContext.Request.QueryString[OrganizationProperty];
                    break;
                case PropertyLocation.Default:
                default:
                    organizationId = (string)(httpContext.Request.RequestContext.RouteData.Values[OrganizationProperty] ??
                                              httpContext.Request.Params[OrganizationProperty] ??
                                              httpContext.Request.QueryString[OrganizationProperty] ??
                                              httpContext.Request.Form[OrganizationProperty]);
                    break;
            }

            if(string.IsNullOrWhiteSpace(organizationId))
                throw new ArgumentException("The OrganizationId could not be extracted from the OrganizationProperty");
            return (Roles?.Split(',').Any(global => httpContext.User.IsInRole(global)) ?? false) ||
                    OrganizationRoles.Split(',').Any(role => httpContext.User.IsInRole(role, organizationId));
        }
    }
}
