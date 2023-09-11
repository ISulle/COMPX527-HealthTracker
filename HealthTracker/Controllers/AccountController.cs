using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace HealthTracker.Controllers;

public class AccountController : Controller
{
    public async Task SignOut()
    {
        // Initiate signout for cookie based authentication scheme
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        // Initiate signout for OpenID authentication scheme
        await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
    }

    public ActionResult SignOutSuccessful()
    {
        return RedirectToAction("Index", "Home");
    }
}

