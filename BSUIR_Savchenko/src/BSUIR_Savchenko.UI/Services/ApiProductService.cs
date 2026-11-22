using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BSUIR_Savchenko.Domain.Entities;
using BSUIR_Savchenko.Domain.Models;
using BSUIR_Savchenko.UI.Models;

namespace BSUIR_Savchenko.UI.Services;

public class ApiProductService(HttpClient client) : IProductService
{
    public string ImageUrl(Book book)
    {
        var path = book.Image ?? "/images/placeholder.svg";
        return new Uri(client.BaseAddress!, path.TrimStart('/')).ToString();
    }

    public Task<ResponseData<ListModel<Book>>> GetProductListAsync(string? categoryNormalizedName, int pageNo = 1)
    {
        var path = $"api/books?pageNo={pageNo}";
        if (!string.IsNullOrWhiteSpace(categoryNormalizedName))
            path += $"&categoryNormalizedName={Uri.EscapeDataString(categoryNormalizedName)}";
        return SendAsync<ListModel<Book>>(() => client.GetAsync(path));
    }

    public Task<ResponseData<Book>> GetProductByIdAsync(int id) =>
        SendAsync<Book>(() => client.GetAsync($"api/books/{id}"));

    public async Task<ResponseData<Book>> CreateProductAsync(BookFormModel book, IFormFile? image)
    {
        using var form = BuildForm(book, image);
        return await SendAsync<Book>(() => client.PostAsync("api/books", form));
    }

    public async Task<ResponseData<Book>> UpdateProductAsync(int id, BookFormModel book, IFormFile? image)
    {
        using var form = BuildForm(book, image);
        return await SendAsync<Book>(() => client.PutAsync($"api/books/{id}", form));
    }

    public Task<ResponseData<bool>> DeleteProductAsync(int id) =>
        SendAsync<bool>(() => client.DeleteAsync($"api/books/{id}"));

    private static MultipartFormDataContent BuildForm(BookFormModel book, IFormFile? image)
    {
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(book.Name), "Name");
        content.Add(new StringContent(book.Description), "Description");
        content.Add(new StringContent(book.CategoryId.ToString(CultureInfo.InvariantCulture)), "CategoryId");
        content.Add(new StringContent(book.Price.ToString(CultureInfo.InvariantCulture)), "Price");

        if (image is not null && image.Length > 0)
        {
            var fileContent = new StreamContent(image.OpenReadStream());
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(image.ContentType);
            content.Add(fileContent, "ImageFile", Path.GetFileName(image.FileName));
        }
        return content;
    }

    private async Task<ResponseData<T>> SendAsync<T>(Func<Task<HttpResponseMessage>> request)
    {
        try
        {
            using var response = await request();
            var data = await response.Content.ReadFromJsonAsync<ResponseData<T>>();
            if (!response.IsSuccessStatusCode)
                return ResponseData<T>.Error(data?.ErrorMessage ?? $"Ошибка API ({(int)response.StatusCode}).");
            return data ?? ResponseData<T>.Error("Пустой ответ API.");
        }
        catch (HttpRequestException)
        {
            return ResponseData<T>.Error("API недоступен. Запустите проект BSUIR_Savchenko.API.");
        }
        catch (TaskCanceledException)
        {
            return ResponseData<T>.Error("Превышено время ожидания API.");
        }
        catch (System.Text.Json.JsonException)
        {
            return ResponseData<T>.Error("Сервис API вернул некорректные данные.");
        }
    }
}
