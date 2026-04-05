using InventoryMVC.Domain.Entities;
using InventoryMVC.Infrastructure;
using InventoryMVC.WebMVC.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

// Allows storing DateTime values as 'timestamp without time zone' in PostgreSQL
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IDataPortServiceFactory<InventoryItem>,
    InventoryItemDataPortServiceFactory>();
builder.Services.AddScoped<IDataPortServiceFactory<ResponsiblePerson>,
    ResponsiblePersonDataPortServiceFactory>();

builder.Services.AddDbContext<InventoryContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString(nameof(InventoryContext)),
        o => o.MigrationsAssembly(typeof(Program).GetTypeInfo().Assembly.GetName().Name)
    ));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllers(); // attribute-routed API controllers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
