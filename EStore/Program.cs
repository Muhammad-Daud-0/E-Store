using EStore.Models;
using EStore.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

// Add services to the container
builder.Services.AddControllersWithViews();

// Add DbContext with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Add Authentication with Google OAuth
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    // DefaultChallengeScheme is omitted so that unauthorized requests are redirected to local login page instead of Google OAuth
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

// Add custom services (using Singleton since it maintains an in-memory concurrent store)
builder.Services.AddSingleton<IInMemoryCartService, InMemoryCartService>();

// Add Data Seeding Service
builder.Services.AddScoped<DataSeedingService>();

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
});

var app = builder.Build();

// Database initialization and seeding
using (var scope = app.Services.CreateScope())
{
    var dataSeedingService = scope.ServiceProvider.GetRequiredService<DataSeedingService>();
    await dataSeedingService.SeedAllDataAsync();

}

// Configure the HTTP request pipeline
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
