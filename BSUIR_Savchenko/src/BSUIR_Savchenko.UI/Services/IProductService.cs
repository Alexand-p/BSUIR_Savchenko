using BSUIR_Savchenko.Domain.Entities;
using BSUIR_Savchenko.Domain.Models;
using BSUIR_Savchenko.UI.Models;

namespace BSUIR_Savchenko.UI.Services;

public interface IProductService
{
    Task<ResponseData<ListModel<Book>>> GetProductListAsync(string? categoryNormalizedName, int pageNo = 1);
    Task<ResponseData<Book>> GetProductByIdAsync(int id);
    Task<ResponseData<Book>> CreateProductAsync(BookFormModel book, IFormFile? image);
    Task<ResponseData<Book>> UpdateProductAsync(int id, BookFormModel book, IFormFile? image);
    Task<ResponseData<bool>> DeleteProductAsync(int id);
    string ImageUrl(Book book);
}
