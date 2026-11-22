using System.Net.Http.Json;
using BSUIR_Savchenko.Domain.Entities;
using BSUIR_Savchenko.Domain.Models;

namespace BSUIR_Savchenko.UI.Services;

public class ApiCategoryService(HttpClient client) : ICategoryService
{
    public async Task<ResponseData<List<Category>>> GetCategoriesAsync()
    {
        try
        {
            using var response = await client.GetAsync("api/categories");
            var result = await response.Content.ReadFromJsonAsync<ResponseData<List<Category>>>();
            if (!response.IsSuccessStatusCode)
                return ResponseData<List<Category>>.Error(result?.ErrorMessage ?? $"Ошибка API ({(int)response.StatusCode}).");
            return result ?? ResponseData<List<Category>>.Error("Не удалось прочитать ответ API.");
        }
        catch (HttpRequestException)
        {
            return ResponseData<List<Category>>.Error("API недоступен. Запустите проект BSUIR_Savchenko.API.");
        }
        catch (TaskCanceledException)
        {
            return ResponseData<List<Category>>.Error("Превышено время ожидания API.");
        }
        catch (System.Text.Json.JsonException)
        {
            return ResponseData<List<Category>>.Error("Сервис API вернул некорректные данные.");
        }
    }
}
