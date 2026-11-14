
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace WalletWise.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
                .WithRedirectUri(returnUrl ?? Url.Action("Index", "Expenses")!)
                .Build();

            return Challenge(
                authenticationProperties,
                Auth0Constants.AuthenticationScheme);
        }

        [HttpGet]
        public IActionResult Logout()
        {
            var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
                .WithRedirectUri(Url.Action("Index", "Home", null, Request.Scheme)!)
                .Build();

            return SignOut(
                authenticationProperties,
                Auth0Constants.AuthenticationScheme,
                Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
