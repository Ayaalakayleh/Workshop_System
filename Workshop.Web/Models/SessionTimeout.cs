using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace Workshop.Web.Models
{
    public class SessionTimeout : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;

            string controllerName = context.RouteData.Values["controller"].ToString().ToLower();
            string actionName = context.RouteData.Values["action"].ToString().ToLower();

            // Get session values in ASP.NET Core
            var userInfo = httpContext.Session.GetString("UserInfo");
            var companyInfo = httpContext.Session.GetString("CompanyInfo");
            var branchInfo = httpContext.Session.GetString("BranchInfo");

            bool isAuthenticationIndex = controllerName == "authentication" && actionName == "index";

            if (!isAuthenticationIndex)
            {
                // Ignore login/logout actions
                if (!actionName.StartsWith("login") && !actionName.StartsWith("logout"))
                {
                    if (userInfo == null || companyInfo == null || branchInfo == null)
                    {
                        // Sign out using ASP.NET Core Identity system
                        httpContext.SignOutAsync();

                        // Clear session
                        httpContext.Session.Clear();

                        context.Result = new RedirectToRouteResult(new RouteValueDictionary {
                        { "controller", "authentication" },
                        { "action", "LogoutProgrammatically" }
                    });

                        return;
                    }
                }
            }
            else // (Authentication / Index)
            {
                if (userInfo != null && companyInfo == null)
                {
                    context.Result = new RedirectToRouteResult(new RouteValueDictionary {
                    { "controller", "authentication" },
                    { "action", "SelectCompany" }
                });
                    return;
                }
                else if (userInfo != null && branchInfo == null)
                {
                    context.Result = new RedirectToRouteResult(new RouteValueDictionary {
                    { "controller", "authentication" },
                    { "action", "SelectBranch" }
                });
                    return;
                }
                else if (userInfo != null)
                {
                    context.Result = new RedirectToRouteResult(new RouteValueDictionary {
                    { "controller", "QuickAccess" },
                    { "action", "Index" }
                });
                    return;
                }
            }

            base.OnActionExecuting(context);
        }
    }

}
