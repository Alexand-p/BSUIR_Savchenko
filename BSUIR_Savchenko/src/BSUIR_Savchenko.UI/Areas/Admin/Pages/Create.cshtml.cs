using BSUIR_Savchenko.UI.Models;
using BSUIR_Savchenko.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BSUIR_Savchenko.UI.Areas.Admin.Pages;

public class CreateModel(IProductService products, ICategoryService categories) : PageModel
{
    [BindProperty]
    public BookFormModel Input { get; set; } = new();

    [BindProperty]
    public IFormFile? ImageFile { get; set; }

    public List<SelectListItem> Categories { get; private set; } = new();

    public async Task OnGetAsync() => await LoadCategoriesAsync();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync();
            return Page();
        }
        var result = await products.CreateProductAsync(Input, ImageFile);
        if (result.Successfull) return RedirectToPage("./Index");
        ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Не удалось создать книгу.");
        await LoadCategoriesAsync();
        return Page();
    }

    private async Task LoadCategoriesAsync()
    {
        var result = await categories.GetCategoriesAsync();
        Categories = (result.Data ?? new()).Select(category => new SelectListItem(category.Name, category.Id.ToString())).ToList();
        if (!result.Successfull) ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Не удалось получить категории.");
    }
}
