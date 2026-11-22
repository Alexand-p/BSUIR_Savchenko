using Microsoft.AspNetCore.Mvc;

namespace BSUIR_Savchenko.UI.Controllers;

public class AccountController : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult LogOut()
    {
        TempData["Message"] = "Демонстрационный выход выполнен.";
        return RedirectToAction("Index", "Home");
    }
}
