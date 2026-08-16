using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using NutriWeb.Data;
using NutriWeb.Models;
using NutriWeb.Services;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args
});

// Отключаем FileSystemWatcher (ReloadOnChange) для предотвращения ошибки лимита inotify в Linux/Docker
builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
{
    foreach (var source in config.Sources.OfType<Microsoft.Extensions.Configuration.Json.JsonConfigurationSource>())
    {
        source.ReloadOnChange = false;
    }
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Автоматическое переключение между PostgreSQL (Neon) и SQLite (локально)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (!string.IsNullOrEmpty(connectionString) && (connectionString.StartsWith("postgres") || connectionString.StartsWith("Host=")))
    {
        options.UseNpgsql(connectionString);
    }
    else
    {
        options.UseSqlite(connectionString ?? "Data Source=nutriweb.db");
    }
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddTransient<IEmailSender, EmailSender>();

var app = builder.Build();

// ИНИЦИАЛИЗАЦИЯ БАЗЫ ДАННЫХ, РОЛЕЙ И АККАУНТА АДМИНИСТРАТОРА
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // 1. Автоматическое применение миграций и создание таблиц
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ошибка при авто-применении миграций базы данных.");
    }

    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    // 2. Создаем роль Admin
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    // 3. Создаем аккаунт администратора по умолчанию
    var adminEmail = "admin@nutriweb.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
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
app.MapRazorPages();

app.Run();