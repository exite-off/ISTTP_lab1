using InventoryMVC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryMVC.Infrastructure.EntityConfigurations;

internal class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.HasKey(i => i.Id);

        builder.HasIndex(i => i.InventoryNumber).IsUnique();

        builder.Property(i => i.InventoryNumber).IsRequired();
        builder.Property(i => i.Name).IsRequired().HasMaxLength(200);
        builder.Property(i => i.EntryDate).IsRequired();
        builder.Property(i => i.Price).IsRequired().HasPrecision(18, 2);
        builder.Property(i => i.Currency).IsRequired().HasMaxLength(10).HasDefaultValue("UAH");
        builder.Property(i => i.Status).IsRequired().HasMaxLength(50);

        builder.HasOne(i => i.Category)
            .WithMany(c => c.InventoryItems)
            .HasForeignKey(i => i.CategoryId);

        builder.HasOne(i => i.Room)
            .WithMany(r => r.InventoryItems)
            .HasForeignKey(i => i.RoomId);

        builder.HasOne(i => i.Vendor)
            .WithMany(v => v.InventoryItems)
            .HasForeignKey(i => i.VendorId);

        builder.HasMany(i => i.Logs)
            .WithOne(l => l.InventoryItem)
            .HasForeignKey(l => l.InventoryItemId);
    }
}
