using System.Globalization;
using BSUIR_Savchenko.UI.Services;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var apiUrl = builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5101/";
builder.Services.AddHttpClient<IProductService, ApiProductService>(client => client.BaseAddress = new Uri(apiUrl));
builder.Services.AddHttpClient<ICategoryService, ApiCategoryService>(client => client.BaseAddress = new Uri(apiUrl));

var app = builder.Build();
app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.Run();
