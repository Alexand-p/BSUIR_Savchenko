using BSUIR_Savchenko.Domain.Entities;
using BSUIR_Savchenko.Domain.Models;

namespace BSUIR_Savchenko.UI.Services;

public interface ICategoryService
{
    Task<ResponseData<List<Category>>> GetCategoriesAsync();
}
