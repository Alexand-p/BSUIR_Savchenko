using BSUIR_Savchenko.Domain.Entities;
using BSUIR_Savchenko.UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BSUIR_Savchenko.UI.Areas.Admin.Pages;

[Authorize(Roles = "POWER-USER")]
public class DeleteModel(IProductService products) : PageModel
{
    public Book Book { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var result = await products.GetProductByIdAsync(id);
        if (!result.Successfull || result.Data is null) return NotFound();
        Book = result.Data;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var result = await products.DeleteProductAsync(id);
        if (result.Successfull) return RedirectToPage("./Index");
        ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Не удалось удалить книгу.");
        await OnGetAsync(id);
        return Page();
    }
}
