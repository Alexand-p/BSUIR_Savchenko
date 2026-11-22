using BSUIR_Savchenko.UI.Models;
using BSUIR_Savchenko.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BSUIR_Savchenko.UI.Controllers;

public class ProductController(IProductService products, ICategoryService categories, IConfiguration config) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? categoryNormalizedName, int pageNo = 1)
    {
        var list = await products.GetProductListAsync(categoryNormalizedName, pageNo);
        var groups = await categories.GetCategoriesAsync();
        var model = new CatalogViewModel
        {
            Books = list.Data?.Items ?? new(),
            Categories = groups.Data ?? new(),
            CategoryNormalizedName = categoryNormalizedName,
            CurrentPage = list.Data?.CurrentPage ?? 1,
            TotalPages = list.Data?.TotalPages ?? 1,
            Error = !list.Successfull ? list.ErrorMessage : (!groups.Successfull ? groups.ErrorMessage : null),
            ApiBaseUrl = config["Api:BaseUrl"] ?? "http://localhost:5101/"
        };
        return View(model);
    }
}
