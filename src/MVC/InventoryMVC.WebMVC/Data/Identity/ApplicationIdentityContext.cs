using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InventoryMVC.WebMVC.Data.Identity;

public class ApplicationIdentityContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationIdentityContext(DbContextOptions<ApplicationIdentityContext> options)
        : base(options) { }
}
