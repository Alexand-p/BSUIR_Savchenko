using System.Globalization;
using BSUIR_Savchenko.API.Data;
using BSUIR_Savchenko.API.Services;
using Microsoft.EntityFrameworkCore;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

var dataFolder = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(dataFolder);
var database = Path.Combine(dataFolder, "catalog.db");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite($"Data Source={database}"));
builder.Services.AddScoped<ImageStorage>();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbInitializer.InitializeAsync(db);
}

app.UseStaticFiles();
app.MapControllers();
app.Run();
