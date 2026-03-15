using InventoryMVC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryMVC.Infrastructure.EntityConfigurations;

internal class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Name).IsRequired().HasMaxLength(150);
        builder.Property(v => v.Address).HasMaxLength(200);
        builder.Property(v => v.ContactPhone).IsRequired().HasMaxLength(20);
        builder.Property(v => v.Email).HasMaxLength(100);
    }
}
