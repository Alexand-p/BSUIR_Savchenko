using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication;
using BSUIR_Savchenko.Domain.Entities;
using BSUIR_Savchenko.Domain.Models;
using BSUIR_Savchenko.UI.Models;

namespace BSUIR_Savchenko.UI.Services;

public class ApiProductService(HttpClient client, IHttpContextAccessor httpContext) : IProductService
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
        return SendAsync<ListModel<Book>>(new HttpRequestMessage(HttpMethod.Get, path));
    }

    public Task<ResponseData<Book>> GetProductByIdAsync(int id) =>
        SendAsync<Book>(new HttpRequestMessage(HttpMethod.Get, $"api/books/{id}"));

    public Task<ResponseData<Book>> CreateProductAsync(BookFormModel book, IFormFile? image) =>
        SendAsync<Book>(new HttpRequestMessage(HttpMethod.Post, "api/books")
        {
            Content = BuildForm(book, image)
        }, true);

    public Task<ResponseData<Book>> UpdateProductAsync(int id, BookFormModel book, IFormFile? image) =>
        SendAsync<Book>(new HttpRequestMessage(HttpMethod.Put, $"api/books/{id}")
        {
            Content = BuildForm(book, image)
        }, true);

    public Task<ResponseData<bool>> DeleteProductAsync(int id) =>
        SendAsync<bool>(new HttpRequestMessage(HttpMethod.Delete, $"api/books/{id}"), true);

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

    private async Task<ResponseData<T>> SendAsync<T>(HttpRequestMessage request, bool requiresToken = false)
    {
        using (request)
        {
            try
            {
                if (requiresToken)
                {
                    var token = await (httpContext.HttpContext?.GetTokenAsync("access_token")
                        ?? Task.FromResult<string?>(null));
                    if (string.IsNullOrWhiteSpace(token))
                        return ResponseData<T>.Error("Для изменения данных войдите в учётную запись.");
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                using var response = await client.SendAsync(request);
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    return ResponseData<T>.Error("Срок действия входа истёк. Войдите повторно.");
                if (response.StatusCode == HttpStatusCode.Forbidden)
                    return ResponseData<T>.Error("Недостаточно прав: требуется роль POWER-USER.");
                ResponseData<T>? payload;
                try
                {
                    payload = await response.Content.ReadFromJsonAsync<ResponseData<T>>();
                }
                catch (System.Text.Json.JsonException)
                {
                    return ResponseData<T>.Error($"Некорректный ответ API ({(int)response.StatusCode}).");
                }
                if (!response.IsSuccessStatusCode)
                    return ResponseData<T>.Error(payload?.ErrorMessage ?? $"Ошибка API ({(int)response.StatusCode}).");
                return payload ?? ResponseData<T>.Error("Пустой ответ API.");
            }
            catch (HttpRequestException)
            {
                return ResponseData<T>.Error("API недоступен. Запустите проект BSUIR_Savchenko.API.");
            }
            catch (TaskCanceledException)
            {
                return ResponseData<T>.Error("Превышено время ожидания API.");
            }
        }
    }
}
