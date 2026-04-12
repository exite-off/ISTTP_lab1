using InventoryMVC.Domain.Entities;
using InventoryMVC.Infrastructure;
using InventoryMVC.WebMVC.Data.Identity;
using InventoryMVC.WebMVC.Extensions;
using InventoryMVC.WebMVC.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

// Stores DateTime as 'timestamp without time zone' in PostgreSQL
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// ── Razor Pages required by Identity UI ──────────────────────────────────────
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

// ── Domain DB context ─────────────────────────────────────────────────────────
builder.Services.AddDbContext<InventoryContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString(nameof(InventoryContext)),
        o => o.MigrationsAssembly(typeof(Program).GetTypeInfo().Assembly.GetName().Name)
    ));

// ── Identity DB context (same DB, separate ASP.NET Identity tables) ───────────
builder.Services.AddDbContext<ApplicationIdentityContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString(nameof(ApplicationIdentityContext)),
        o => o.MigrationsAssembly(typeof(Program).GetTypeInfo().Assembly.GetName().Name)
    ));

// ── Identity: users + roles, no email confirmation required ──────────────────
builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequiredLength = 8;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationIdentityContext>();

// ── Redirect unauthenticated users to /Identity/Account/Login ────────────────
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath  = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// ── Excel import/export services ─────────────────────────────────────────────
builder.Services.AddScoped<IDataPortServiceFactory<InventoryItem>,
    InventoryItemDataPortServiceFactory>();
builder.Services.AddScoped<IDataPortServiceFactory<ResponsiblePerson>,
    ResponsiblePersonDataPortServiceFactory>();

var app = builder.Build();

// ── Seed roles and default users in development ───────────────────────────────
await app.InitializeRolesAsync();
if (app.Environment.IsDevelopment())
{
    await app.InitializeDefaultUsersAsync(
        app.Configuration.GetSection("IdentityDefaults:Admin"),
        app.Configuration.GetSection("IdentityDefaults:Monitor"));
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Order matters: Authentication must come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();   // attribute-routed API controllers (Charts)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();    // Identity UI pages (/Identity/Account/Login, etc.)

app.Run();
