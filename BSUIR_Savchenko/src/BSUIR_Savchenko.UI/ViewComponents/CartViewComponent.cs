using BSUIR_Savchenko.UI.Models;
using Microsoft.AspNetCore.Mvc;

namespace BSUIR_Savchenko.UI.ViewComponents;

public class CartViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        return View(new CartSummaryModel(0m, 0));
    }
}
