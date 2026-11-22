using BSUIR_Savchenko.UI.Models;
using Microsoft.AspNetCore.Mvc;

namespace BSUIR_Savchenko.UI.Controllers;

public class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index() => View(new HomeFormModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(HomeFormModel model)
    {
        if (ModelState.IsValid)
        {
            ViewBag.Message = "Данные успешно отправлены на сервер.";
        }

        return View(model);
    }
}
