namespace InventoryMVC.WebMVC.Extensions;

public static class RoleNames
{
    public const string Admin   = "Admin";
    public const string Monitor = "Monitor";

    // All roles — used for seeding and iteration
    public static readonly string[] All = [Admin, Monitor];
}
