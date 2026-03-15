using InventoryMVC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryMVC.Infrastructure.EntityConfigurations;

internal class ResponsiblePersonConfiguration : IEntityTypeConfiguration<ResponsiblePerson>
{
    public void Configure(EntityTypeBuilder<ResponsiblePerson> builder)
    {
        builder.HasKey(rp => rp.Id);

        builder.Property(rp => rp.FullName).IsRequired().HasMaxLength(150);
        builder.Property(rp => rp.Position).IsRequired().HasMaxLength(100);
        builder.Property(rp => rp.Email).IsRequired().HasMaxLength(100);

        // Restrict delete — a person with assigned items can't be removed
        builder.HasMany(rp => rp.InventoryItems)
            .WithOne(i => i.ResponsiblePerson)
            .HasForeignKey(i => i.ResponsiblePersonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
