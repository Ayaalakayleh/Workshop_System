using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Workshop.Web.Controllers
{
    public class LanguageController : Controller
    {
        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(culture))
                return BadRequest();

            culture = culture.Trim().ToLower();

            if (culture != "ar" && culture != "en")
                culture = "en";

            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                HttpOnly = false,
                Path = "/"
            };

            Response.Cookies.Append("Language", culture, cookieOptions);

            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                cookieOptions
            );

            return Ok();
        }
    }
}
