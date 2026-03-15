using InventoryMVC.Domain.Entities;
using InventoryMVC.Infrastructure.EntityConfigurations;
using Microsoft.EntityFrameworkCore;

namespace InventoryMVC.Infrastructure;

public class InventoryContext : DbContext
{
    public InventoryContext(DbContextOptions<InventoryContext> options) : base(options) { }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<ResponsiblePerson> ResponsiblePersons => Set<ResponsiblePerson>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<InventoryLog> InventoryLogs => Set<InventoryLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new DepartmentConfiguration());
        modelBuilder.ApplyConfiguration(new ResponsiblePersonConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        modelBuilder.ApplyConfiguration(new VendorConfiguration());
        modelBuilder.ApplyConfiguration(new RoomConfiguration());
        modelBuilder.ApplyConfiguration(new InventoryItemConfiguration());
        modelBuilder.ApplyConfiguration(new InventoryLogConfiguration());
    }
}
