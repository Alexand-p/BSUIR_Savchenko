using BSUIR_Savchenko.Domain.Entities;

namespace BSUIR_Savchenko.UI.Models;

public class CatalogViewModel
{
    public List<Book> Books { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public string? CategoryNormalizedName { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public string? Error { get; set; }
    public string ApiBaseUrl { get; set; } = string.Empty;

    public string ImageUrl(Book book) =>
        new Uri(new Uri(ApiBaseUrl), (book.Image ?? "/images/placeholder.svg").TrimStart('/')).ToString();
}
