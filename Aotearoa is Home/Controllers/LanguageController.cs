using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Aotearoa_is_Home.Controllers;

public class LanguageController : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SetLanguage(string culture, string? returnUrl)
    {
        var supportedCultures = new[] { "en-NZ", "mi-NZ" };

        if (!supportedCultures.Contains(culture))
        {
            culture = "en-NZ";
        }

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(
                new RequestCulture(culture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                HttpOnly = true,
                SameSite = SameSiteMode.Lax
            });

        if (string.IsNullOrWhiteSpace(returnUrl) ||
            !Url.IsLocalUrl(returnUrl))
        {
            returnUrl = "/";
        }

        return LocalRedirect(returnUrl);
    }
}
