using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NKDUPVS_React.Server.Controllers;

/// <summary>
/// Handles account-related operations such as sign-in and sign-out.
/// </summary>
[AllowAnonymous]
public class AccountController : Controller
{
    /// <summary>
    /// Initiates the Google authentication process.
    /// </summary>
    /// <returns>Sign-in result</returns>
    [HttpGet]
    public IActionResult SignIn()
    {
        var redirectUrl = Url.Action(nameof(HomeController.Index), "Home");
        return Challenge(new AuthenticationProperties { RedirectUri = redirectUrl }, GoogleDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Signs the user out and redirects to the home page.
    /// </summary>
    /// <returns>Sign-out result</returns>
    [HttpGet]
    public new IActionResult SignOut()
    {
        var callbackUrl = Url.Action(nameof(HomeController.Index), "Home", values: null, protocol: Request.Scheme);
        return SignOut(new AuthenticationProperties { RedirectUri = callbackUrl },
            CookieAuthenticationDefaults.AuthenticationScheme, GoogleDefaults.AuthenticationScheme);
    }
}