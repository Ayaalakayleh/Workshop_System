using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Workshop.Web.Models
{
    public class CustomAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly int[] _allowedRoles;

        public CustomAuthorizeAttribute(params int[] roles)
        {
            _allowedRoles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Resolve services HERE (inside request scope)
            var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<CustomAuthorizeAttribute>>();

            var httpContext = context.HttpContext;

            var token = httpContext.Request.Cookies["Token"];
            var roleSession = httpContext.Session.GetString("Role");
            var permissionSession = httpContext.Session.GetString("Permission");

            if (string.IsNullOrEmpty(token) || !VeirifyJWTToken(token, config, logger))
            {
                RedirectToLogout(context, config);
                return;
            }

            // Admin role bypass
            if (roleSession == "1")
                return;

            var listPermissions = permissionSession?
                .Split(',')
                .Select(int.Parse)
                .ToList() ?? new List<int>();

            bool authorized = _allowedRoles.Any(role => listPermissions.Contains(role));

            if (!authorized)
                RedirectToLogout(context, config);
        }

        public bool VeirifyJWTToken(string token, IConfiguration config, ILogger logger)
        {
            try
            {
                var key = Encoding.UTF8.GetBytes(config["ApiSettings:Secret"]);
                var tokenHandler = new JwtSecurityTokenHandler();

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return validatedToken != null;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Invalid JWT Token");
                return false;
            }
        }

        private void RedirectToLogout(AuthorizationFilterContext context, IConfiguration config)
        {
            var logoutUrl = config["ApiSettings:SystemLogOut"] ?? "/Authentication/Logout";
            context.Result = new RedirectResult(logoutUrl);
        }
    }

}
