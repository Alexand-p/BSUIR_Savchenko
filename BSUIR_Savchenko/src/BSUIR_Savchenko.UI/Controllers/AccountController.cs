using BSUIR_Savchenko.UI.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BSUIR_Savchenko.UI.Controllers;

public class AccountController(AvatarStorage avatars) : Controller
{
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        return Challenge(new AuthenticationProperties
        {
            RedirectUri = Url.Action("Index", "Home")
        }, OpenIdConnectDefaults.AuthenticationScheme);
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction(nameof(Profile));
        var properties = new AuthenticationProperties { RedirectUri = Url.Action(nameof(Profile)) };
        properties.Items["registration"] = "true";
        return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
    }

    [Authorize]
    [HttpGet]
    public IActionResult Profile() => View();

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(3 * 1024 * 1024)]
    public async Task<IActionResult> Profile(IFormFile? avatar, CancellationToken cancellationToken)
    {
        try
        {
            await avatars.SaveAsync(User.FindFirst("sub")?.Value, avatar, cancellationToken);
            TempData["Message"] = "Аватар сохранён.";
            return RedirectToAction(nameof(Profile));
        }
        catch (InvalidDataException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View();
        }
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult LogOut()
    {
        return SignOut(new AuthenticationProperties
        {
            RedirectUri = Url.Action("Index", "Home")
        }, CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
    }
}
