using System.Globalization;
using System.Security.Claims;
using BSUIR_Savchenko.Domain.Security;
using BSUIR_Savchenko.UI.Middleware;
using BSUIR_Savchenko.UI.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Serilog;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<AvatarStorage>();

var keycloak = builder.Configuration.GetSection("Keycloak");
var authority = keycloak["Authority"]
    ?? throw new InvalidOperationException("Не задан адрес Keycloak.");
var secret = keycloak["ClientSecret"];
if (string.IsNullOrWhiteSpace(secret))
    throw new InvalidOperationException("Не задан Keycloak:ClientSecret. Выполните scripts/setup-keycloak.ps1.");

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(55);
        options.SlidingExpiration = false;
    })
    .AddOpenIdConnect(options =>
    {
        options.Authority = authority;
        options.ClientId = "bsuir-ui";
        options.ClientSecret = secret;
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.UsePkce = true;
        options.PushedAuthorizationBehavior = PushedAuthorizationBehavior.Disable;
        options.SaveTokens = true;
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.GetClaimsFromUserInfoEndpoint = false;
        options.MapInboundClaims = false;
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "preferred_username",
            RoleClaimType = ClaimTypes.Role
        };
        options.Events = new OpenIdConnectEvents
        {
            OnRedirectToIdentityProvider = context =>
            {
                if (context.Properties.Items.TryGetValue("registration", out var value) && value == "true")
                    context.ProtocolMessage.Prompt = "create";
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                KeycloakRoleClaims.AddRoleClaims(context.Principal);
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

var apiUrl = builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5101/";
builder.Services.AddHttpClient<IProductService, ApiProductService>(client => client.BaseAddress = new Uri(apiUrl));
builder.Services.AddHttpClient<ICategoryService, ApiCategoryService>(client => client.BaseAddress = new Uri(apiUrl));

var logPath = Path.Combine(builder.Environment.ContentRootPath, "logs", "non-success-.log");
builder.Services.AddSingleton<Serilog.ILogger>(_ => new LoggerConfiguration()
    .MinimumLevel.Warning()
    .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
    .CreateLogger());

var app = builder.Build();
app.UseMiddleware<NonSuccessRequestLoggingMiddleware>();
app.UseExceptionHandler(error => error.Run(async context =>
{
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await context.Response.WriteAsync("Ошибка приложения.");
}));
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.Run();
