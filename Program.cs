using DotNetEnv;
using EStore.Models;
using EStore.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
})
.AddGoogle(options =>
{
    options.ClientId = configuration["Authentication:Google:ClientId"] ?? "";
    options.ClientSecret = configuration["Authentication:Google:ClientSecret"] ?? "";
});

builder.Services.AddScoped<ICartService, DatabaseCartService>();

builder.Services.AddScoped<DataSeedingService>();

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
});

var app = builder.Build();

// Database initialization and seeding (skipped via env var `SKIP_DATA_SEED=true`)
var skipSeeding = configuration.GetValue<bool>("SKIP_DATA_SEED", false);
if (!skipSeeding)
{
    using (var scope = app.Services.CreateScope())
    {
        var dataSeedingService = scope.ServiceProvider.GetRequiredService<DataSeedingService>();
        await dataSeedingService.SeedAllDataAsync();
    }
}
else
{
    app.Logger.LogInformation("Data seeding skipped because SKIP_DATA_SEED is set to true.");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
