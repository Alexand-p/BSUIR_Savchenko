using System.Globalization;
using System.Security.Claims;
using BSUIR_Savchenko.API.Data;
using BSUIR_Savchenko.API.Services;
using BSUIR_Savchenko.Domain.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

var authority = builder.Configuration["Keycloak:Authority"]
    ?? throw new InvalidOperationException("Не задан адрес сервера Keycloak.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authority;
        options.Audience = "bsuir-api";
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            NameClaimType = "preferred_username",
            RoleClaimType = ClaimTypes.Role
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                KeycloakRoleClaims.AddRoleClaims(context.Principal);
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

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
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
