namespace Workshop.Web.Models
{
    public static class PermissionHelper
    {
        private static IHttpContextAccessor _httpContextAccessor;

        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public static bool UserHasPermission(int permission)
        {
            var ctx = _httpContextAccessor.HttpContext;

            if (ctx == null)
                return false;

            var permissionString = ctx.Session.GetString("Permission");
            if (string.IsNullOrEmpty(permissionString))
                return false;

            var userInfo = ctx.Session.GetString("UserInfo");
            if (string.IsNullOrEmpty(userInfo))
                return false;

            int requiredPermissions = permission;
            var userPermissions = permissionString
                  .Split(',', StringSplitOptions.RemoveEmptyEntries)
                  .Select(p => int.Parse(p.Trim()))
                  .ToList();

            return userPermissions.Contains(requiredPermissions);
        }
    }

}
