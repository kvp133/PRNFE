using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PRNFE.MVC.Models;

namespace PRNFE.MVC.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;
        private readonly string[] _permissions;

        public AuthorizeAttribute(string[] roles = null, string[] permissions = null)
        {
            _roles = roles;
            _permissions = permissions;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var userInfo = context.HttpContext.Items["UserInfo"] as JwtTokenModel;
            
            if (userInfo == null)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            // Check roles if specified
            if (_roles != null && _roles.Length > 0)
            {
                if (!_roles.Contains(userInfo.role))
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }

            // Check permissions if specified
            if (_permissions != null && _permissions.Length > 0)
            {
                var userPermissions = userInfo.permission ?? new List<string>();
                if (!_permissions.Any(p => userPermissions.Contains(p)))
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireAdminAttribute : AuthorizeAttribute
    {
        public RequireAdminAttribute() : base(new[] { "Quản lý trọ" })
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireLandlordAttribute : AuthorizeAttribute
    {
        public RequireLandlordAttribute() : base(new[] { "Quản lý trọ" })
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireTenantAttribute : AuthorizeAttribute
    {
        public RequireTenantAttribute() : base(new[] { "Người thuê trọ" })
        {
        }
    }
} 