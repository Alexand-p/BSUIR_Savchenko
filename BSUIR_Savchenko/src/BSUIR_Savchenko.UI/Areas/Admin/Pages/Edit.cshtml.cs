using BSUIR_Savchenko.UI.Models;
using BSUIR_Savchenko.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BSUIR_Savchenko.UI.Areas.Admin.Pages;

public class EditModel(IProductService products, ICategoryService categories) : PageModel
{
    [BindProperty]
    public BookFormModel Input { get; set; } = new();

    [BindProperty]
    public IFormFile? ImageFile { get; set; }

    public List<SelectListItem> Categories { get; private set; } = new();
    public string? CurrentImageUrl { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var result = await products.GetProductByIdAsync(id);
        if (!result.Successfull || result.Data is null) return NotFound();
        var book = result.Data;
        Input = new BookFormModel
        {
            Name = book.Name,
            Description = book.Description,
            CategoryId = book.CategoryId,
            Price = book.Price
        };
        CurrentImageUrl = products.ImageUrl(book);
        await LoadCategoriesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync();
            return Page();
        }
        var result = await products.UpdateProductAsync(id, Input, ImageFile);
        if (result.Successfull) return RedirectToPage("./Index");
        ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Не удалось сохранить изменения.");
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
