using System.Net.Http.Json;
using BSUIR_Savchenko.Domain.Entities;
using BSUIR_Savchenko.Domain.Models;

namespace BSUIR_Savchenko.Blazor.Services;

public sealed class DataService(IHttpClientFactory httpClientFactory, IConfiguration config)
{
    public event Action? Changed;
    public IReadOnlyList<Category> Categories { get; private set; } = [];
    public IReadOnlyList<Book> Products { get; private set; } = [];
    public string? CategoryNormalizedName { get; private set; }
    public int CurrentPage { get; private set; } = 1;
    public int TotalPages { get; private set; } = 1;
    public bool Loading { get; private set; }
    public string? Error { get; private set; }
    public string ApiBaseUrl => config["Api:BaseUrl"] ?? "http://localhost:5101/";

    public async Task InitializeAsync()
    {
        try
        {
            var response = await httpClientFactory.CreateClient("CatalogApi")
                .GetFromJsonAsync<ResponseData<List<Category>>>("api/categories");
            if (response is null || !response.Successfull || response.Data is null)
                throw new InvalidOperationException(response?.ErrorMessage ?? "Не удалось получить категории.");
            Categories = response.Data;
            Changed?.Invoke();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException)
        {
            Error = "Ошибка загрузки категорий: " + ex.Message;
            Changed?.Invoke();
            return;
        }
        await GetProductListAsync();
    }

    public Task SelectCategoryAsync(string? normalizedName)
    {
        CategoryNormalizedName = string.IsNullOrWhiteSpace(normalizedName) ? null : normalizedName;
        CurrentPage = 1;
        return GetProductListAsync();
    }

    public Task SelectPageAsync(int page)
    {
        if (page < 1 || page > TotalPages || page == CurrentPage)
            return Task.CompletedTask;
        CurrentPage = page;
        return GetProductListAsync();
    }

    public async Task GetProductListAsync()
    {
        Loading = true;
        Error = null;
        Changed?.Invoke();
        try
        {
            var path = $"api/books?pageNo={CurrentPage}";
            if (CategoryNormalizedName is not null)
                path += "&categoryNormalizedName=" + Uri.EscapeDataString(CategoryNormalizedName);
            var response = await httpClientFactory.CreateClient("CatalogApi")
                .GetFromJsonAsync<ResponseData<ListModel<Book>>>(path);
            if (response is null || !response.Successfull || response.Data is null)
                throw new InvalidOperationException(response?.ErrorMessage ?? "Не удалось получить список книг.");
            Products = response.Data.Items;
            CurrentPage = response.Data.CurrentPage;
            TotalPages = response.Data.TotalPages;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException)
        {
            Products = [];
            Error = "Ошибка получения книг: " + ex.Message;
        }
        finally
        {
            Loading = false;
            Changed?.Invoke();
        }
    }
}
