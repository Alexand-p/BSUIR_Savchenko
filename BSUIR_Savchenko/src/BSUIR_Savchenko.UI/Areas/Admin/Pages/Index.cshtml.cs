using BSUIR_Savchenko.Domain.Entities;
using BSUIR_Savchenko.UI.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BSUIR_Savchenko.UI.Areas.Admin.Pages;

public class IndexModel(IProductService products) : PageModel
{
    public List<Book> Books { get; private set; } = new();
    public int CurrentPage { get; private set; } = 1;
    public int TotalPages { get; private set; } = 1;
    public string? Error { get; private set; }

    public string ImageUrl(Book book) => products.ImageUrl(book);

    public async Task OnGetAsync(int pageNo = 1)
    {
        var result = await products.GetProductListAsync(null, pageNo);
        Books = result.Data?.Items ?? new();
        CurrentPage = result.Data?.CurrentPage ?? 1;
        TotalPages = result.Data?.TotalPages ?? 1;
        Error = result.Successfull ? null : result.ErrorMessage;
    }
}
