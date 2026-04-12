using InventoryMVC.WebMVC.Data.Identity;
using Microsoft.AspNetCore.Identity;

namespace InventoryMVC.WebMVC.Extensions;

public static class IdentityWebApplicationExtensions
{
    // Simple DTO to deserialize seed user config
    private record UserInfo(string Username, string Password)
    {
        public UserInfo() : this(string.Empty, string.Empty) { }
    }

    // Creates the user if missing, then ensures they have the requested roles
    private static async Task AddUserIfNotExistsAsync(
        UserManager<ApplicationUser> userManager,
        ILogger logger,
        string username,
        string password,
        ICollection<string> roles)
    {
        var user = await userManager.FindByEmailAsync(username);
        if (user is null)
        {
            user = new ApplicationUser { UserName = username, Email = username };
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                logger.LogError("Failed to create {Username}: {Errors}",
                    username, string.Join(", ", result.Errors.Select(e => e.Description)));
                return;
            }
            logger.LogInformation("{Username} created", username);
        }
        else
        {
            logger.LogInformation("{Username} already exists, skipping creation", username);
        }

        var existingRoles = await userManager.GetRolesAsync(user);
        foreach (var role in roles.Where(r => !existingRoles.Contains(r)))
        {
            await userManager.AddToRoleAsync(user, role);
            logger.LogInformation("{Username} assigned role '{Role}'", username, role);
        }
    }

    // Creates all roles defined in RoleNames if they don't exist yet
    public static async Task InitializeRolesAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var roleName in RoleNames.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole { Name = roleName });
                app.Logger.LogInformation("Role '{Role}' created", roleName);
            }
        }
    }

    // Seeds one admin and one monitor from appsettings
    public static async Task InitializeDefaultUsersAsync(
        this WebApplication app,
        IConfiguration? adminConfig,
        IConfiguration? monitorConfig)
    {
        using var scope = app.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var adminInfo = adminConfig?.Get<UserInfo>();
        if (adminInfo is not null)
            await AddUserIfNotExistsAsync(userManager, app.Logger,
                adminInfo.Username, adminInfo.Password, [RoleNames.Admin]);

        var monitorInfo = monitorConfig?.Get<UserInfo>();
        if (monitorInfo is not null)
            await AddUserIfNotExistsAsync(userManager, app.Logger,
                monitorInfo.Username, monitorInfo.Password, [RoleNames.Monitor]);
    }
}
